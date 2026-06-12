using KYC.Application.Interfaces;
using MediatR;

namespace KYC.Application.Cases;

public class GenerateAmlReportCommandHandler(IAmlComplianceReportService service)
    : IRequestHandler<GenerateAmlReportCommand, Guid>
{
    public async Task<Guid> Handle(GenerateAmlReportCommand request, CancellationToken ct)
    {
        var report = await service.GenerateAnnualReportAsync(request.Year, request.RequestedBy, ct);
        return report.Id;
    }
}
