using KYC.Domain.Entities;

namespace KYC.Application.Interfaces;

public interface IRcbePartyVerificationService
{
    Task VerifyCasePartiesAsync(KycCase kycCase, CancellationToken ct = default);
}
