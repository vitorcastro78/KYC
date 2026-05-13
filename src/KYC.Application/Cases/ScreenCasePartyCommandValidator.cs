using FluentValidation;

namespace KYC.Application.Cases;

public class ScreenCasePartyCommandValidator : AbstractValidator<ScreenCasePartyCommand>
{
    public ScreenCasePartyCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.PartyId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
    }
}
