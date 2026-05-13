namespace KYC.Application.Interfaces;

public record IcijMatch(string EntityName, string Jurisdiction, string SourceDataset, string? Details);

public interface IIcijOffshoreService
{
    Task<IReadOnlyList<IcijMatch>> SearchAsync(string name, CancellationToken ct = default);
}
