using KYC.Domain.Entities;

namespace KYC.Application.Interfaces;

public interface IDocumentConsistencyChecker
{
    IReadOnlyList<RiskSignal> Check(KycCase kycCase);
}
