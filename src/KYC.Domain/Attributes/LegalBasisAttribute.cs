namespace KYC.Domain.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public sealed class LegalBasisAttribute(string legalRef) : Attribute
{
    public string LegalRef { get; } = legalRef;
}
