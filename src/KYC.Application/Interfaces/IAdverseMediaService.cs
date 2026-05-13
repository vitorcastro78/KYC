using KYC.Application.Models;

namespace KYC.Application.Interfaces;

public interface IAdverseMediaService
{
    Task<AdverseMediaResult> ScanAsync(string entityName, string? nif = null, CancellationToken ct = default);
}
