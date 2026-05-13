namespace KYC.Application.Models;

public record ConsistencyCheckResult(
    bool IsConsistent,
    IReadOnlyList<string> Issues,
    int CoherenceScore);
