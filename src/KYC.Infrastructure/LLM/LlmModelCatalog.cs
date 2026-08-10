using System.Net.Http.Json;
using System.Text.Json;
using KYC.Application.Interfaces;
using KYC.Application.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace KYC.Infrastructure.LLM;

public sealed class LlmModelCatalog(
    IContextMemoryWikiClient wikiClient,
    IOptions<ContextMemoryOptions> cmOptions,
    IConfiguration configuration,
    ILogger<LlmModelCatalog> log) : ILlmModelCatalog
{
    public async Task<LlmModelsListResult> ListAsync(CancellationToken cancellationToken = default)
    {
        var configured = LlmOptions.GetModel(configuration);

        if (cmOptions.Value.IsConfigured)
        {
            try
            {
                return await wikiClient.ListModelsAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "ContextMemory /v1/models failed; returning configured model only.");
            }
        }

        return new LlmModelsListResult
        {
            Provider = "contextmemory",
            Data = { new LlmModelInfo { Id = configured, OwnedBy = "active" } }
        };
    }
}
