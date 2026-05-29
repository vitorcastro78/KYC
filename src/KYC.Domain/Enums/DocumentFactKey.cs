using KYC.Domain.Attributes;

namespace KYC.Domain.Enums;

public enum DocumentFactKey
{
    [LegalBasis("Lei83/2017-Art24-n1-a")] CompanyName = 0,
    [LegalBasis("Lei83/2017-Art24-n1-b")] Nif = 1,
    [LegalBasis("Lei83/2017-Art24-n1-c")] Address = 2,
    [LegalBasis("Lei83/2017-Art25")] Cae = 3,
    [LegalBasis("Lei83/2017-Art35")] Iban = 4,
    [LegalBasis("Lei83/2017-Art35")] Revenue = 5,
    [LegalBasis("Lei83/2017-Art35")] Equity = 6,
    [LegalBasis("Lei83/2017-Art24")] DocumentDate = 7,
    [LegalBasis("Lei83/2017-Art35")] Summary = 8
}
