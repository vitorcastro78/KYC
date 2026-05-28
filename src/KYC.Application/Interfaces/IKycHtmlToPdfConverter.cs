namespace KYC.Application.Interfaces;

public interface IKycHtmlToPdfConverter
{
    Task<byte[]> ConvertAsync(string htmlDocument, CancellationToken ct = default);
}
