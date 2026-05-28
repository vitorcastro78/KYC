using FluentValidation;
using KYC.Application.Common;

namespace KYC.Application.Cases;

public class UploadCaseDocumentCommandValidator : AbstractValidator<UploadCaseDocumentCommand>
{
    public UploadCaseDocumentCommandValidator()
    {
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(512);
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Content).NotNull();
        RuleFor(x => x.FileName)
            .Must(DocumentUploadRules.IsAllowedExtension)
            .WithMessage("Extensão de ficheiro não permitida.");
    }
}
