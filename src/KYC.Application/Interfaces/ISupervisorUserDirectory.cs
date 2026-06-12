namespace KYC.Application.Interfaces;

public interface ISupervisorUserDirectory
{
    Task<IReadOnlyList<SupervisorUserDto>> ListSupervisorsAsync(CancellationToken ct = default);
}

public record SupervisorUserDto(string Id, string DisplayName, string Email);
