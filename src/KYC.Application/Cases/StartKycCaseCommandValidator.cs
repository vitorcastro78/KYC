using FluentValidation;
using KYC.Application.Common;

namespace KYC.Application.Cases;

public class StartKycCaseCommandValidator : AbstractValidator<StartKycCaseCommand>
{
    public StartKycCaseCommandValidator()
    {
        RuleFor(x => x.Nif)
            .Must(n => NifSanitizer.TryNormalizeCaseKey(n, out _))
            .WithMessage(
                "Indique um NIF/NIPC/LEI (6–20 caracteres alfanuméricos) ou o nome da entidade (2–32 caracteres úteis após remover espaços e pontuação; ex.: nome comercial).");
        RuleFor(x => x.RequestedBy).NotEmpty();
        RuleFor(x => x.RequestedAmount.Amount).GreaterThan(0);
        RuleFor(x => x.RequestedAmount.Currency).NotEmpty().MaximumLength(8);
    }
}
