using KYC.Domain.Entities;

namespace KYC.Application.Interfaces;

public interface IBdpRpbExporter
{
    byte[] ToOfficialXml(AmlComplianceReport report);
    byte[] ToInternalJson(AmlComplianceReport report);
    BdpRpbValidationResult ValidateOfficialXml(byte[] xml);
}

public sealed record BdpRpbValidationResult(bool IsValid, IReadOnlyList<string> Errors);
