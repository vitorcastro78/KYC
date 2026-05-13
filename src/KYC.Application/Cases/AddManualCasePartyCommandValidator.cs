using FluentValidation;
using KYC.Application.Common;
using KYC.Domain.Enums;

namespace KYC.Application.Cases;

public class AddManualCasePartyCommandValidator : AbstractValidator<AddManualCasePartyCommand>
{
    public AddManualCasePartyCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(512);
        RuleFor(x => x.Nif)
            .Must(n => string.IsNullOrWhiteSpace(n) || NifSanitizer.TryNormalize(n!, out _))
            .WithMessage("NIF / identificador inválido.");
        RuleFor(x => x.OwnershipPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.UboDepthLevel).GreaterThanOrEqualTo(0).LessThanOrEqualTo(20);
        RuleFor(x => x.Nationality).MaximumLength(128).When(x => x.Nationality is not null);
    }
}
