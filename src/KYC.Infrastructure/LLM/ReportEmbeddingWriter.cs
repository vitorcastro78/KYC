using KYC.Application.Interfaces;
using KYC.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using KYC.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace KYC.Infrastructure.LLM;

public sealed class ReportEmbeddingWriter(
    KycDbContext db,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<ReportEmbeddingWriter> log) : IReportEmbeddingWriter
{
    private const int DefaultDimensions = 2048;

    private int TargetDimensions =>
        int.TryParse(configuration["LLM:EmbeddingDimensions"], out var n) && n > 0 ? n : DefaultDimensions;

    public Task StoreChunksAsync(Guid kycCaseId, IReadOnlyList<(string Chunk, float[] Vector)> chunks, CancellationToken ct = default)
    {
        var dim = TargetDimensions;
        foreach (var (chunk, vector) in chunks)
        {
            var normalized = NormalizeDimensions(vector, dim);
            db.ReportEmbeddings.Add(new ReportEmbedding
            {
                Id = Guid.NewGuid(),
                KycCaseId = kycCaseId,
                ContentChunk = chunk,
                Embedding = new HalfVector(ToHalfArray(normalized)),
                CreatedAt = DateTime.UtcNow
            });
        }

        // Persistência fica a cargo do chamador (ex.: KycCaseRepository.UpdateAsync) para um único SaveChanges com o caso.
        return Task.CompletedTask;
    }

    public async Task ClearEmbeddingsAsync(Guid kycCaseId, CancellationToken ct = default)
    {
        await db.ReportEmbeddings
            .Where(e => e.KycCaseId == kycCaseId)
            .ExecuteDeleteAsync(ct);
    }

    /// <summary>Gera embeddings via Ollama (qwen3-embedding:8b por defeito); fallback determinístico para pseudo-vector.</summary>
    public async Task EmbedReportTextAsync(Guid kycCaseId, string markdown, CancellationToken ct = default)
    {
        var parts = markdown.Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var dim = TargetDimensions;
        var list = new List<(string, float[])>();
        foreach (var part in parts.Take(32))
        {
            var vec = await TryEmbedAsync(part, ct) ?? PseudoVector(part, dim);
            list.Add((part, vec));
        }

        await StoreChunksAsync(kycCaseId, list, ct);
    }

    private async Task<float[]?> TryEmbedAsync(string text, CancellationToken ct)
    {
        try
        {
            var client = httpClientFactory.CreateClient("ollama");
            var model = configuration["LLM:EmbeddingModel"] ?? "qwen3-embedding:8b";
            using var res = await client.PostAsJsonAsync("/api/embeddings", new { model, prompt = text }, ct);
            if (!res.IsSuccessStatusCode) return null;
            var doc = await res.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(cancellationToken: ct);
            var arr = doc.GetProperty("embedding");
            var result = new float[arr.GetArrayLength()];
            var i = 0;
            foreach (var el in arr.EnumerateArray())
                result[i++] = (float)el.GetDouble();
            return result;
        }
        catch (Exception ex)
        {
            log.LogDebug(ex, "Embedding call failed; using pseudo vector.");
            return null;
        }
    }

    private static float[] PseudoVector(string text, int dim)
    {
        var rng = new Random(text.GetHashCode(StringComparison.Ordinal));
        var v = new float[dim];
        for (var i = 0; i < dim; i++)
            v[i] = (float)rng.NextDouble();
        return v;
    }

    private static float[] NormalizeDimensions(float[] source, int targetDim)
    {
        if (source.Length == targetDim) return source;
        var output = new float[targetDim];
        var copy = Math.Min(source.Length, targetDim);
        Array.Copy(source, output, copy);
        return output;
    }

    private static Half[] ToHalfArray(float[] source)
    {
        var result = new Half[source.Length];
        for (var i = 0; i < source.Length; i++)
            result[i] = (Half)source[i];
        return result;
    }
}
