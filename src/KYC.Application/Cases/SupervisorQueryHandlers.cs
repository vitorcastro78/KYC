using KYC.Application.Interfaces;
using MediatR;

namespace KYC.Application.Cases;

public record GetSupervisorUsersQuery : IRequest<IReadOnlyList<SupervisorUserDto>>;

public class GetSupervisorUsersQueryHandler(ISupervisorUserDirectory directory)
    : IRequestHandler<GetSupervisorUsersQuery, IReadOnlyList<SupervisorUserDto>>
{
    public Task<IReadOnlyList<SupervisorUserDto>> Handle(
        GetSupervisorUsersQuery request,
        CancellationToken cancellationToken) =>
        directory.ListSupervisorsAsync(cancellationToken);
}
