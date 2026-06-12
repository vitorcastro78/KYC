using KYC.Domain.Entities;

namespace KYC.Application.Interfaces;

public interface ICaseDocumentRepository
{
    Task<CaseDocument?> GetByIdAsync(Guid documentId, CancellationToken ct = default);
    Task UpdateAsync(CaseDocument document, CancellationToken ct = default);
}
