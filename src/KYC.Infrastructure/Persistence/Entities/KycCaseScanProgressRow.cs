namespace KYC.Infrastructure.Persistence.Entities;

public class KycCaseScanProgressRow
{
    public Guid KycCaseId { get; set; }
    public int TotalScans { get; set; }
    public int CompletedScans { get; set; }
    public int FailedScans { get; set; }
}
