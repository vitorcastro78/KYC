using KYC.Domain.Enums;

namespace KYC.Application.Filtering;

public record KycCaseFilter(
    KycStatus? Status = null,
    string? SearchNif = null,
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1,
    int PageSize = 20);
