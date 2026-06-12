using KYC.Application.Interfaces;
using KYC.Domain.Enums;

namespace KYC.Web.Endpoints;

/// <summary>
/// API de integração B2B (sem dependência FinSight). Consumida por adaptadores externos.
/// </summary>
public static class IntegrationEndpoints
{
    public static void MapIntegrationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/integration").WithTags("Integration");

        group.MapGet("/entities/{entityId}/summary", async (
            string entityId,
            IKycCaseRepository repository,
            CancellationToken ct) =>
        {
            var kycCase = await repository.GetByNifAsync(entityId.Trim(), ct);
            if (kycCase is null)
                return Results.NotFound();

            var pep = kycCase.Parties.Any(p => p.IsPep);
            var sanctions = kycCase.Parties.Any(p => p.IsSanctioned)
                || kycCase.RiskSignals.Any(s => s.Type == SignalType.Sanction && s.IsConfirmed);

            return Results.Ok(new EntitySummaryResponse(
                EntityId: kycCase.Nif,
                Status: kycCase.Status.ToString(),
                RiskTier: MapRiskTier(kycCase.Score?.Level),
                Pep: pep,
                SanctionsMatch: sanctions,
                LastReviewedAt: kycCase.LastReviewedAt));
        }).AllowAnonymous();
    }

    private static string MapRiskTier(RiskLevel? level) => level switch
    {
        RiskLevel.Low => "Low",
        RiskLevel.Medium => "Medium",
        RiskLevel.High => "High",
        RiskLevel.Critical => "Critical",
        _ => "Medium"
    };

    private sealed record EntitySummaryResponse(
        string EntityId,
        string Status,
        string RiskTier,
        bool Pep,
        bool SanctionsMatch,
        DateTime? LastReviewedAt);
}
