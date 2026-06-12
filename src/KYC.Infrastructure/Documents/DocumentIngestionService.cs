using System.Net.Http.Json;
using System.Text.Json;
using KYC.Application.Interfaces;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.Documents;

public sealed class DocumentIngestionService(
    ICaseDocumentRepository documents,
    ICaseDocumentStorage storage,
    IKycCaseRepository cases,
    IKycCaseMessageBus messageBus,
    IKycCaseRealtimeNotifier notifier,
    DocumentVisionExtractor visionExtractor,
    DocumentFieldExtractor fieldExtractor,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<DocumentIngestionService> logger) : IDocumentIngestionService
{
    public async Task ProcessDocumentAsync(Guid documentId, CancellationToken ct = default)
    {
        var document = await documents.GetByIdAsync(documentId, ct)
                       ?? throw new KeyNotFoundException("Documento não encontrado.");

        if (document.IngestionStatus is DocumentIngestionStatus.Completed)
            return;

        document.MarkProcessing();
        await documents.UpdateAsync(document, ct);
        await notifier.NotifyDocumentIngestionUpdatedAsync(document.KycCaseId, document.Id, document.IngestionStatus, ct);

        try
        {
            await using var stream = await storage.OpenReadAsync(document.StorageRelativePath, ct);
            var format = DocumentFormatDetector.Detect(document.FileName, stream);
            var text = await ExtractTextAsync(format, stream, document.FileName, ct);

            if (string.IsNullOrWhiteSpace(text))
            {
                document.MarkFailed("Não foi possível extrair texto do documento.");
                await documents.UpdateAsync(document, ct);
                await notifier.NotifyDocumentIngestionUpdatedAsync(document.KycCaseId, document.Id, document.IngestionStatus, ct);
                return;
            }

            var (payload, rawJson, promptHash) = await fieldExtractor.ExtractStructuredAsync(
                text,
                CallLlmJsonAsync,
                ct);

            var (facts, parties) = DocumentExtractionMapper.MapToEntities(document.Id, document.KycCaseId, payload);
            document.ReplaceExtractedData(facts, parties);
            document.MarkCompleted(
                text,
                rawJson,
                configuration["LLM:LocalModel"] ?? "qwen3.5:9b",
                promptHash);

            await documents.UpdateAsync(document, ct);

            var kyc = await cases.GetByIdAsync(document.KycCaseId, ct);
            if (kyc is not null)
            {
                kyc.AppendAudit(AuditEntry.Create(
                    kyc.Id,
                    "DocumentExtracted",
                    "System",
                    "LLM",
                    document.FileName,
                    promptHash));
                await cases.UpdateAsync(kyc, ct);
            }

            await notifier.NotifyDocumentIngestionUpdatedAsync(document.KycCaseId, document.Id, document.IngestionStatus, ct);
            await messageBus.PublishCaseRescreenAsync(document.KycCaseId, "System", ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha na ingestão do documento {DocumentId}", documentId);
            document.MarkFailed(ex.Message);
            await documents.UpdateAsync(document, ct);
            await notifier.NotifyDocumentIngestionUpdatedAsync(document.KycCaseId, document.Id, document.IngestionStatus, ct);
        }
    }

    private async Task<string> ExtractTextAsync(
        DocumentFormat format,
        Stream stream,
        string fileName,
        CancellationToken ct)
    {
        if (format is DocumentFormat.Jpeg or DocumentFormat.Png or DocumentFormat.Tiff)
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, ct);
            var mime = format switch
            {
                DocumentFormat.Jpeg => "image/jpeg",
                DocumentFormat.Png => "image/png",
                _ => "image/tiff"
            };
            return await visionExtractor.ExtractTextFromImageAsync(ms.ToArray(), mime, ct);
        }

        if (format is DocumentFormat.Unknown)
            throw new InvalidOperationException("Formato de documento não suportado.");

        var result = DocumentTextExtractor.ExtractFromFormat(format, stream);
        if (!result.UsedVisionFallback)
            return result.Text;

        throw new InvalidOperationException(
            "PDF parece ser digitalizado (scan). Envie JPEG/PNG ou um PDF com texto selecionável.");
    }

    private async Task<string?> CallLlmJsonAsync(string system, string user, CancellationToken ct)
    {
        if (!await IsOllamaReachableAsync(ct))
            return null;

        var model = configuration["LLM:LocalModel"] ?? "qwen3.5:9b";
        var timeoutSeconds = Math.Clamp(configuration.GetValue("LLM:DocumentExtractionTimeoutSeconds", 120), 30, 600);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        var payload = new
        {
            model,
            stream = false,
            options = new { num_predict = 1024, temperature = 0.1 },
            messages = new object[]
            {
                new { role = "system", content = system },
                new { role = "user", content = user }
            }
        };

        var client = httpClientFactory.CreateClient("ollama");
        using var response = await client.PostAsJsonAsync("/api/chat", payload, cts.Token);
        response.EnsureSuccessStatusCode();
        var doc = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cts.Token);
        return doc.GetProperty("message").GetProperty("content").GetString();
    }

    private async Task<bool> IsOllamaReachableAsync(CancellationToken ct)
    {
        try
        {
            var client = httpClientFactory.CreateClient("ollama-health");
            using var response = await client.GetAsync("/api/tags", ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
