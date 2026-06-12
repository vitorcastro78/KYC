namespace KYC.Application.Interfaces;

public interface IPeriodicReviewScheduler
{
    /// <summary>Publica re-triagem para casos com revisão vencida até à data indicada.</summary>
    Task<int> PublishDueReviewsAsync(DateTime dueBeforeUtc, CancellationToken ct = default);
}
