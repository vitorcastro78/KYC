namespace KYC.Application.Dtos;

public record KycCaseScanProgressDto(
    Guid CaseId,
    int TotalScans,
    int CompletedScans,
    int FailedScans)
{
    public int PercentComplete =>
        TotalScans <= 0 ? 0 : Math.Clamp((int)Math.Round((double)CompletedScans * 100 / TotalScans), 0, 100);
}

