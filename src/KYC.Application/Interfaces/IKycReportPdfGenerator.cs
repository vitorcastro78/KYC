namespace KYC.Application.Interfaces;

public interface IKycReportPdfGenerator
{
    Task<byte[]> GenerateAsync(Guid caseId, CancellationToken ct = default);
}
