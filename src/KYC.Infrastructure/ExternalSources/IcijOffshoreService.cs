using KYC.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.ExternalSources;

public sealed class IcijOffshoreService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<IcijOffshoreService> log) : IIcijOffshoreService
{
    public async Task<IReadOnlyList<IcijMatch>> SearchAsync(string name, CancellationToken ct = default)
    {
        var enabled = configuration.GetValue("ExternalSources:Icij:Enabled", true);
        if (!enabled || string.IsNullOrWhiteSpace(name))
            return [];

        var baseUrl = configuration["ExternalSources:Icij:GraphQlUrl"]
                      ?? "https://offshoreleaks.icij.org/api/graphql";
        var client = httpClientFactory.CreateClient("icij");

        var query = """
            query Search($term: String!) {
              search(term: $term, limit: 5) {
                nodes { name jurisdiction entityType sourceId }
              }
            }
            """;

        try
        {
            using var res = await client.PostAsJsonAsync(baseUrl, new
            {
                query,
                variables = new { term = name.Trim() }
            }, ct);

            if (!res.IsSuccessStatusCode)
                return [];

            var doc = await res.Content.ReadFromJsonAsync<IcijGraphQlResponse>(cancellationToken: ct);
            return doc?.Data?.Search?.Nodes?.Select(n => new IcijMatch(
                n.Name ?? name,
                n.Jurisdiction ?? "",
                n.EntityType ?? "entity",
                n.SourceId)).ToList() ?? [];
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "ICIJ search failed for {Name}.", name);
            return [];
        }
    }

    private sealed class IcijGraphQlResponse
    {
        public IcijData? Data { get; set; }
    }

    private sealed class IcijData
    {
        public IcijSearch? Search { get; set; }
    }

    private sealed class IcijSearch
    {
        public List<IcijNode>? Nodes { get; set; }
    }

    private sealed class IcijNode
    {
        public string? Name { get; set; }
        public string? Jurisdiction { get; set; }
        public string? EntityType { get; set; }
        public string? SourceId { get; set; }
    }
}
