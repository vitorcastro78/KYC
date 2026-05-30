using KYC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace KYC.Infrastructure.Persistence;

/// <summary>Impede DELETE de versões regulatórias activas (PAC, scoring, DPIA).</summary>
public sealed class RegulatoryVersionSaveChangesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Guard(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Guard(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void Guard(DbContext? context)
    {
        if (context is null)
            return;

        foreach (var entry in context.ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted))
        {
            switch (entry.Entity)
            {
                case CustomerAcceptancePolicy p when ReadIsActive(entry, nameof(CustomerAcceptancePolicy.IsActive)):
                    throw new InvalidOperationException(
                        $"Não é permitido apagar a PAC activa (versão {p.Version}). Crie uma versão sucessora.");
                case ScoringEngineConfig c when ReadIsActive(entry, nameof(ScoringEngineConfig.IsActive)):
                    throw new InvalidOperationException(
                        $"Não é permitido apagar o motor de scoring activo (versão {c.Version}).");
                case DpiaRecord d when ReadIsActive(entry, nameof(DpiaRecord.IsActive)):
                    throw new InvalidOperationException(
                        $"Não é permitido apagar a DPIA activa (versão {d.Version}).");
            }
        }
    }

    private static bool ReadIsActive(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, string propertyName)
    {
        var prop = entry.Property(propertyName);
        return prop.CurrentValue is true;
    }
}
