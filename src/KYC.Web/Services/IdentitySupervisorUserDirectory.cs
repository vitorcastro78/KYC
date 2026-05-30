using KYC.Application.Interfaces;
using KYC.Web.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KYC.Web.Services;

public sealed class IdentitySupervisorUserDirectory(UserManager<ApplicationUser> userManager)
    : ISupervisorUserDirectory
{
    public async Task<IReadOnlyList<SupervisorUserDto>> ListSupervisorsAsync(CancellationToken ct = default)
    {
        var users = await userManager.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.FullName)
            .ToListAsync(ct);

        var supervisors = new List<SupervisorUserDto>();
        foreach (var user in users)
        {
            if (!await userManager.IsInRoleAsync(user, "KYC.Supervisor")
                && !await userManager.IsInRoleAsync(user, "KYC.Admin"))
                continue;

            var id = user.Email ?? user.UserName ?? user.Id;
            supervisors.Add(new SupervisorUserDto(id, user.FullName, user.Email ?? id));
        }

        return supervisors;
    }
}
