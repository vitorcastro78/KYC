using System.Text;

namespace KYC.Application.Common;

public static class NifSanitizer
{
    /// <summary>Limite alinhado com <c>kyc_cases.Nif</c> / <c>case_parties.Nif</c> (varchar 32).</summary>
    public const int MaxCaseKeyLength = 32;

    /// <summary>
    /// Normaliza identificador comercial: remove separadores, mantém letras/dígitos.
    /// Aceita NIF/NIPC PT (9 dígitos) ou identificadores internacionais (6–20 caracteres alfanuméricos), ex. LEI, CNPJ, NUIT.
    /// </summary>
    public static bool TryNormalize(string input, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var cleaned = new string(input.Where(char.IsLetterOrDigit).ToArray());
        if (cleaned.Length is < 6 or > 20)
            return false;

        normalized = cleaned.ToUpperInvariant();
        return true;
    }

    /// <summary>
    /// Chave para abrir um caso: NIF/NIPC/LEI (6–20) ou nome comercial reduzido a alfanumérico (2–32, truncado).
    /// Permite abrir caso só com o nome quando ainda não há identificador fiscal.
    /// </summary>
    public static bool TryNormalizeCaseKey(string input, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var trimmed = input.Trim().Normalize(NormalizationForm.FormC);
        var cleaned = new string(trimmed.Where(char.IsLetterOrDigit).ToArray());
        if (cleaned.Length < 2)
            return false;
        if (cleaned.Length > MaxCaseKeyLength)
            cleaned = cleaned[..MaxCaseKeyLength];

        normalized = cleaned.ToUpperInvariant();
        return true;
    }
}
