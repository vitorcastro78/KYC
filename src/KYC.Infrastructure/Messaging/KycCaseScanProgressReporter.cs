using KYC.Application.Common;
using KYC.Application.Interfaces;

namespace KYC.Infrastructure.Messaging;

/// <summary>Persiste e notifica a mesma percentagem/módulo (BD + SignalR).</summary>
internal static class KycCaseScanProgressReporter
{
    public static async Task ReportAsync(
        IKycCaseScanProgressRepository progress,
        IKycCaseRealtimeNotifier notifier,
        Guid caseId,
        string module,
        int percentComplete,
        CancellationToken ct)
    {
        var pct = Math.Clamp(percentComplete, 0, 100);
        await progress.UpsertAsync(KycCaseScanProgressScale.UiPercent(caseId, pct), ct);
        await notifier.NotifyScanProgressAsync(caseId, module, pct, ct);
    }
}
