namespace KYC.Infrastructure.Compliance;

public sealed class DataRetentionOptions
{
    public const string SectionName = "DataRetention";

    public bool EnableHostedService { get; set; }
    public int RejectedCaseRetentionYears { get; set; } = 5;
    public int ApprovedCaseRetentionYears { get; set; } = 7;
    public bool AnonymizeRejectedAfterRetention { get; set; } = true;
    public bool MarkApprovedCasesPastRetention { get; set; } = true;
}
