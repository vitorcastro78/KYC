using KYC.Application.Models;

namespace KYC.Application.Interfaces;

public interface IEntityResolutionService
{
    Task<EntityResolutionResult> ResolveByNifAsync(string nif, CancellationToken ct = default);
    Task<UboGraph> BuildUboGraphAsync(string nif, int maxDepth = 5, CancellationToken ct = default);

    /// <summary>Consulta a API GLEIF para exibição no detalhe do caso (sem alterar resolução persistida).</summary>
    Task<GleifCompanySnapshot?> FetchGleifProfileAsync(
        string caseKey,
        string? companyName = null,
        CancellationToken ct = default);

    /// <summary>Perfil GLEIF + entidades relacionadas (pais e filhas) para UI e triagem.</summary>
    Task<GleifEnrichment> FetchGleifEnrichmentAsync(
        string caseKey,
        string? companyName = null,
        CancellationToken ct = default);
}
