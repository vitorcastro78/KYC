using KYC.Application.Interfaces;
using KYC.Domain.Entities;
using KYC.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace KYC.Application.Cases;

public class UploadCaseDocumentCommandHandler(
    IKycCaseRepository repository,
    ICaseDocumentStorage storage,
    IDocumentIngestionQueue ingestionQueue,
    IConfiguration configuration)
    : IRequestHandler<UploadCaseDocumentCommand, Guid>
{
    public async Task<Guid> Handle(UploadCaseDocumentCommand request, CancellationToken cancellationToken)
    {
        var kyc = await repository.GetByIdAsync(request.CaseId, cancellationToken)
                  ?? throw new KeyNotFoundException("Caso não encontrado.");

        if (kyc.Status is KycStatus.Rejected)
            throw new InvalidOperationException("Não é possível carregar documentos num caso rejeitado.");

        if (request.CasePartyId is { } partyId && kyc.Parties.All(p => p.Id != partyId))
            throw new InvalidOperationException("A parte indicada não pertence a este caso.");

        var maxBytes = Math.Clamp(configuration.GetValue("Documents:MaxFileSizeBytes", 26_214_400L), 1_048_576L, 104_857_600L);

        var documentId = Guid.NewGuid();
        var stored = await storage.SaveAsync(kyc.Id, documentId, request.Content, request.FileName, cancellationToken);
        if (stored.SizeBytes > maxBytes)
        {
            await storage.DeleteAsync(stored.StorageRelativePath, cancellationToken);
            throw new InvalidOperationException($"Ficheiro excede o limite de {maxBytes / (1024 * 1024)} MB.");
        }

        var document = CaseDocument.Create(
            kyc.Id,
            request.FileName,
            request.ContentType,
            stored.SizeBytes,
            stored.Sha256,
            stored.StorageRelativePath,
            request.Kind,
            request.ActorId,
            request.CasePartyId);

        kyc.AddDocument(document, request.ActorId);
        await repository.UpdateAsync(kyc, cancellationToken);
        await ingestionQueue.EnqueueAsync(document.Id, cancellationToken);
        return document.Id;
    }
}
