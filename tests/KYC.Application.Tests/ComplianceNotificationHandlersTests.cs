using KYC.Application.Compliance;
using KYC.Application.Interfaces;
using Moq;

namespace KYC.Application.Tests;

public class ComplianceNotificationHandlersTests
{
    [Fact]
    public async Task Sar_submitted_notifies_case_and_supervisors()
    {
        var notifier = new Mock<IKycCaseRealtimeNotifier>();
        var handler = new SarSubmittedNotificationHandler(notifier.Object);
        var caseId = Guid.NewGuid();

        await handler.Handle(
            new SarSubmittedNotification(caseId, "Acme", "UIF-99", true),
            CancellationToken.None);

        notifier.Verify(n => n.NotifyComplianceAlertAsync(
            caseId, "SarSubmitted", It.Is<string>(m => m.Contains("UIF-99")), It.IsAny<CancellationToken>()), Times.Once);
        notifier.Verify(n => n.NotifySupervisorsAsync(
            "SarSubmitted", It.Is<string>(m => m.Contains("URGENTE")), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Identity_verified_notifies_case_group()
    {
        var notifier = new Mock<IKycCaseRealtimeNotifier>();
        var handler = new EntityIdentityVerifiedNotificationHandler(notifier.Object);

        await handler.Handle(
            new EntityIdentityVerifiedNotification(Guid.NewGuid(), Guid.NewGuid(), "João"),
            CancellationToken.None);

        notifier.Verify(n => n.NotifyComplianceAlertAsync(
            It.IsAny<Guid>(), "IdentityVerified", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
