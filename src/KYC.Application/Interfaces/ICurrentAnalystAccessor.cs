namespace KYC.Application.Interfaces;

/// <summary>Identificador do analista autenticado (email ou subject id).</summary>
public interface ICurrentAnalystAccessor
{
    Task<string> GetAnalystIdAsync(CancellationToken ct = default);
}
