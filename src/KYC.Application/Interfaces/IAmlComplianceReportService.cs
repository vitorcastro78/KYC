using KYC.Domain.Entities;

namespace KYC.Application.Interfaces;

public interface IAmlComplianceReportService
{
    Task<AmlComplianceReport> GenerateAnnualReportAsync(int year, string requestedBy, CancellationToken ct = default);
    Task<Stream> ExportRpbAsync(Guid reportId, CancellationToken ct = default);
    Task<Stream> ExportRpbBdpAsync(Guid reportId, CancellationToken ct = default);
    Task<string> SubmitToBdpAsync(Guid reportId, string submittedBy, CancellationToken ct = default);
}

public interface IAmlComplianceReportRepository
{
    Task<AmlComplianceReport?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(AmlComplianceReport report, CancellationToken ct = default);
    Task UpdateAsync(AmlComplianceReport report, CancellationToken ct = default);
    Task<IReadOnlyList<AmlComplianceReport>> ListAsync(CancellationToken ct = default);
}

public interface IDpiaRecordRepository
{
    Task<DpiaRecord?> GetActiveAsync(CancellationToken ct = default);
    Task AddAsync(DpiaRecord record, CancellationToken ct = default);
    Task<IReadOnlyList<DpiaRecord>> ListAsync(CancellationToken ct = default);
}
