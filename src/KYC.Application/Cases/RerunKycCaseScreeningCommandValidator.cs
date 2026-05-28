using FluentValidation;

namespace KYC.Application.Cases;

public class RerunKycCaseScreeningCommandValidator : AbstractValidator<RerunKycCaseScreeningCommand>
{
    public RerunKycCaseScreeningCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty().MaximumLength(256);
    }
}
