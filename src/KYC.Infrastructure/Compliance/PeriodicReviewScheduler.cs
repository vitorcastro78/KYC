using KYC.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.Compliance;

public sealed class PeriodicReviewScheduler(
    IKycCaseRepository cases,
    IKycCaseMessageBus bus,
    ILogger<PeriodicReviewScheduler> log) : IPeriodicReviewScheduler
{
    public async Task<int> PublishDueReviewsAsync(DateTime dueBeforeUtc, CancellationToken ct = default)
    {
        var due = await cases.GetCasesDueForReviewAsync(dueBeforeUtc, ct);
        foreach (var kyc in due)
        {
            log.LogInformation(
                "Revisão periódica agendada para caso {CaseId} — vence {DueDate:yyyy-MM-dd}",
                kyc.Id,
                kyc.NextReviewDue);
            await bus.PublishCaseRescreenAsync(kyc.Id, "System", ct);
        }

        return due.Count;
    }
}
