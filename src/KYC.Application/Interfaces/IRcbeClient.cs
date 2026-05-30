namespace KYC.Application.Interfaces;

public interface IRcbeClient
{
    Task<RcbeCompanyDto?> GetCompanyByNifAsync(string nif, CancellationToken ct = default);
}

public record RcbeCompanyDto(string Nif, string LegalName, string? RegistryId);
