namespace KYC.Application.Models;

public record SanctionsMatch(string ListName, string MatchedName, double Score, string? Details);

public record SanctionsResult(IReadOnlyList<SanctionsMatch> Matches)
{
    public bool IsClear => Matches.Count == 0;
}
