using KYC.Application.Interfaces;

namespace KYC.Infrastructure.Messaging;

/// <summary>
/// Workers não expõem UI de supervisores; satisfaz MediatR sem dependência do KYC.Web.
/// </summary>
public sealed class NoOpSupervisorUserDirectory : ISupervisorUserDirectory
{
    public Task<IReadOnlyList<SupervisorUserDto>> ListSupervisorsAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<SupervisorUserDto>>([]);
}
