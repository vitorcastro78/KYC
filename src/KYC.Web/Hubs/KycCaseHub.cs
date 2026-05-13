using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace KYC.Web.Hubs;

[Authorize(Policy = "Analyst")]
public class KycCaseHub : Hub
{
    public async Task JoinCaseGroup(Guid caseId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, CaseGroup(caseId));

    public async Task LeaveCaseGroup(Guid caseId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, CaseGroup(caseId));

    public static string CaseGroup(Guid caseId) => $"case-{caseId}";
}
