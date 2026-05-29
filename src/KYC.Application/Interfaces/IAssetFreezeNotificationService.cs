namespace KYC.Application.Interfaces;

public interface IAssetFreezeNotificationService
{
    Task<AssetFreezeNotificationResult> NotifyAsync(
        Guid kycCaseId,
        Guid partyId,
        string sanctionListSource,
        string matchReference,
        string notifiedBy,
        CancellationToken ct = default);
}

public record AssetFreezeNotificationResult(
    bool IsSuccess,
    string? ConfirmationNumber,
    string? ErrorMessage,
    DateTime NotifiedAt);
