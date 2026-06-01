using KYC.Application.Interfaces;

namespace KYC.Application.Common;

/// <summary>
/// Convenção de persistência: TotalScans=100 e CompletedScans=percentagem UI (0–100) do pipeline.
/// Valores antigos (TotalScans = nº de partes) mantêm compatibilidade no DTO.
/// </summary>
public static class KycCaseScanProgressScale
{
    public const int UiPercentTotal = 100;

    public static bool UsesUiPercentScale(int totalScans) => totalScans == UiPercentTotal;

    public static KycCaseScanProgressState UiPercent(Guid caseId, int percentComplete, int failedScans = 0) =>
        new(caseId, UiPercentTotal, Math.Clamp(percentComplete, 0, 100), failedScans);
}
