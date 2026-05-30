using KYC.Application.Cases;
using KYC.Application.Interfaces;
using KYC.Domain.Enums;
using KYC.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KYC.Infrastructure.BackgroundJobs;

public sealed class IdentityVerificationPollingService(
    KycDbContext db,
    IIdentityVerificationService identity,
    IMediator mediator,
    ILogger<IdentityVerificationPollingService> log)
{
    public async Task<int> PollPendingSessionsAsync(CancellationToken ct)
    {
        var pending = await db.CaseParties
            .Where(p => p.VerificationStatus == IdentityVerificationStatus.Pending
                        && p.VerificationSessionId != null
                        && !p.VerificationSessionId.StartsWith("local-"))
            .Select(p => new { p.Id, p.VerificationSessionId })
            .Take(20)
            .ToListAsync(ct);

        var processed = 0;
        foreach (var row in pending)
        {
            if (string.IsNullOrWhiteSpace(row.VerificationSessionId))
                continue;

            var result = await identity.GetVerificationResultAsync(row.VerificationSessionId, ct);
            if (result.FailureReason?.Contains("Pending", StringComparison.OrdinalIgnoreCase) == true)
                continue;

            await mediator.Send(new RecordVerificationResultCommand(
                row.Id,
                row.VerificationSessionId,
                result.IsVerified,
                result.FailureReason,
                result.EidasLevel), ct);
            processed++;
        }

        if (processed > 0)
            log.LogDebug("Identity polling processou {Count} sessões.", processed);

        return processed;
    }
}
