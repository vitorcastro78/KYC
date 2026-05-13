namespace KYC.Application.Models;

public record JudicialCaseRef(string Reference, string Court, string Status, DateTime? Date);

public record JudicialResult(IReadOnlyList<JudicialCaseRef> Cases);
