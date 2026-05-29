using KYC.Domain.Entities;
using KYC.Domain.Enums;

namespace KYC.Application.Interfaces;

public interface IIdentityVerificationService
{
    Task<IdentityVerificationSession> InitiateVerificationAsync(
        Guid partyId,
        IdentityVerificationMethod method,
        string entityName,
        string? email,
        CancellationToken ct = default);

    Task<IdentityVerificationResult> GetVerificationResultAsync(string sessionId, CancellationToken ct = default);

    Task RecordPresentialVerificationAsync(
        Guid partyId,
        string analystId,
        string documentReference,
        CancellationToken ct = default);
}

public record IdentityVerificationSession(
    string SessionId,
    string VerificationUrl,
    IdentityVerificationMethod Method,
    DateTime ExpiresAt);

public record IdentityVerificationResult(
    string SessionId,
    bool IsVerified,
    string? FailureReason,
    DateTime VerifiedAt,
    string? LivenessScore,
    string? EidasLevel);
