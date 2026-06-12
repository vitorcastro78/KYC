using KYC.Domain.Enums;

namespace KYC.Web.Services;

public static class ComplianceUiLabels
{
    public static bool RequiresIdentityVerification(EntityRole role) =>
        role is EntityRole.Ubo or EntityRole.BoardMember or EntityRole.Proxy;

    public static string VerificationStatus(IdentityVerificationStatus status) => status switch
    {
        IdentityVerificationStatus.Verified => "Verificado",
        IdentityVerificationStatus.Failed => "Falhou",
        IdentityVerificationStatus.Expired => "Expirado",
        IdentityVerificationStatus.Pending => "Pendente",
        _ => "Por verificar"
    };

    public static string VerificationMethod(IdentityVerificationMethod method) => method switch
    {
        IdentityVerificationMethod.VideoConference => "Videoconferência",
        IdentityVerificationMethod.CMD => "Chave Móvel Digital",
        IdentityVerificationMethod.Presential => "Presencial",
        IdentityVerificationMethod.QualifiedSignature => "Assinatura qualificada",
        IdentityVerificationMethod.NotYetVerified => "Ainda não verificado",
        _ => method.ToString()
    };

    public static string VerificationBadgeClass(IdentityVerificationStatus status) => status switch
    {
        IdentityVerificationStatus.Verified => "bg-success",
        IdentityVerificationStatus.Failed => "bg-danger",
        IdentityVerificationStatus.Expired => "bg-warning text-dark",
        IdentityVerificationStatus.Pending => "bg-primary",
        _ => "bg-secondary"
    };

    public static string SarStatusLabel(SarStatus status) => status switch
    {
        SarStatus.Submitted => "Comunicado à UIF",
        SarStatus.Pending => "SAR pendente",
        SarStatus.NotRequired => "SAR não aplicável",
        _ => "Sem SAR"
    };

    public static string SarBadgeClass(SarStatus status) => status switch
    {
        SarStatus.Submitted => "bg-danger",
        SarStatus.Pending => "bg-warning text-dark",
        SarStatus.NotRequired => "bg-secondary",
        _ => "bg-light text-dark"
    };

    public static string SarListLabel(SarStatus status) => status switch
    {
        SarStatus.Submitted => "UIF",
        SarStatus.Pending => "SAR pend.",
        SarStatus.NotRequired => "N/A",
        _ => "—"
    };

    public static string FormatApproveBlockMessage(string? canApproveMessage)
    {
        if (string.IsNullOrWhiteSpace(canApproveMessage))
            return string.Empty;

        const string prefix = "Aprovação bloqueada:";
        return canApproveMessage.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? canApproveMessage.Trim()
            : $"{prefix} {canApproveMessage.Trim()}";
    }
}
