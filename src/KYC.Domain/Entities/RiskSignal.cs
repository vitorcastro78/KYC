using KYC.Domain.Enums;

namespace KYC.Domain.Entities;

public class RiskSignal
{
    public Guid Id { get; private set; }
    public Guid KycCaseId { get; private set; }
    public Guid? CasePartyId { get; private set; }
    public SignalType Type { get; private set; }
    public SignalSeverity Severity { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;
    public DateTime DetectedAt { get; private set; }
    public DateTime? EventDate { get; private set; }
    public bool IsConfirmed { get; private set; }
    public string? AnalystNotes { get; private set; }

    private RiskSignal()
    {
    }

    public static RiskSignal Create(
        Guid kycCaseId,
        Guid? casePartyId,
        SignalType type,
        SignalSeverity severity,
        string description,
        string source,
        DateTime? eventDate = null)
    {
        return new RiskSignal
        {
            Id = Guid.NewGuid(),
            KycCaseId = kycCaseId,
            CasePartyId = casePartyId,
            Type = type,
            Severity = severity,
            Description = description,
            Source = source,
            DetectedAt = DateTime.UtcNow,
            EventDate = eventDate,
            IsConfirmed = false
        };
    }

    public void Confirm(string analystId, string? notes)
    {
        IsConfirmed = true;
        AnalystNotes = notes;
    }

    public void OverrideConfirmation(bool confirm, string? notes)
    {
        IsConfirmed = confirm;
        AnalystNotes = notes;
    }
}
