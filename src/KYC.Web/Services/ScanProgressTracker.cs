namespace KYC.Web.Services;

public enum ScanProgressHubResult
{
    Updated,
    IgnoredStale,
    Completed,
    Failed
}

/// <summary>Estado unificado da barra de progresso (SignalR primário, BD só em fallback).</summary>
public sealed class ScanProgressTracker
{
    public string DisplayModule { get; private set; } = "—";
    public int Percent { get; private set; }
    public bool InProgress { get; private set; }
    public bool AwaitingFreshProgress { get; private set; }

    public void BeginNewCaseFlow()
    {
        InProgress = true;
        AwaitingFreshProgress = false;
        SetModulePercent("A iniciar", 0);
    }

    public void ApplyNewCaseCreatedHint()
    {
        if (!InProgress)
            return;
        SetModulePercent("EntityResolution", 5);
    }

    public void BeginRescreen()
    {
        InProgress = true;
        AwaitingFreshProgress = true;
        SetModulePercent("A iniciar", 0);
    }

    public void ResetAfterFailure()
    {
        InProgress = false;
        AwaitingFreshProgress = false;
    }

    public ScanProgressHubResult ApplyHubUpdate(string module, int percent)
    {
        if (string.Equals(module, "Erro na triagem", StringComparison.OrdinalIgnoreCase))
        {
            ResetAfterFailure();
            return ScanProgressHubResult.Failed;
        }

        if (!ApplyServerProgress(module, percent, out var completed))
            return ScanProgressHubResult.IgnoredStale;

        return completed ? ScanProgressHubResult.Completed : ScanProgressHubResult.Updated;
    }

    /// <summary>Aplica progresso vindo da BD (polling). Aceita aumentos monótonos mesmo com hub ligado.</summary>
    public bool TryApplyDatabaseFallback(int percentComplete, bool hubConnected = false)
    {
        if (hubConnected && !InProgress)
            return false;

        return ApplyServerProgress(
            ScanProgressLabels.ModuleKeyFromDatabasePercent(percentComplete),
            percentComplete,
            out _);
    }

    private bool ApplyServerProgress(string moduleKey, int percent, out bool completed)
    {
        completed = false;
        var pct = Math.Clamp(percent, 0, 100);

        if (AwaitingFreshProgress && pct >= 100)
            return false;

        if (pct > 0 && pct < Percent && !AwaitingFreshProgress)
            return false;

        SetModulePercent(moduleKey, pct);

        if (pct is 0 or (> 0 and < 100))
        {
            InProgress = true;
            if (pct > 0)
                AwaitingFreshProgress = false;
            return true;
        }

        if (pct >= 100)
        {
            InProgress = false;
            AwaitingFreshProgress = false;
            completed = true;
            return true;
        }

        return true;
    }

    public void ApplyCompletedCaseView()
    {
        InProgress = false;
        AwaitingFreshProgress = false;
        SetModulePercent("Concluído", 100);
    }

    public void ApplyIdleView()
    {
        InProgress = false;
        AwaitingFreshProgress = false;
        DisplayModule = "—";
        Percent = 0;
    }

    public void ApplyStoredProgressView(int percentComplete)
    {
        InProgress = false;
        AwaitingFreshProgress = false;
        if (percentComplete >= 100)
            ApplyCompletedCaseView();
        else if (percentComplete > 0)
            SetModulePercent(ScanProgressLabels.ModuleKeyFromDatabasePercent(percentComplete), percentComplete);
        else
            ApplyIdleView();
    }

    private void SetModulePercent(string moduleKey, int percent)
    {
        DisplayModule = ScanProgressLabels.ToDisplayLabel(moduleKey);
        Percent = Math.Clamp(percent, 0, 100);
    }
}
