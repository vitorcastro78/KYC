using MediatR;

namespace KYC.Application.Compliance;

public sealed record EntityIdentityVerifiedNotification(
    Guid CaseId,
    Guid PartyId,
    string PartyName) : INotification;

public sealed record EntityIdentityVerificationFailedNotification(
    Guid CaseId,
    Guid PartyId,
    string PartyName,
    string? Reason) : INotification;

public sealed record SarSubmittedNotification(
    Guid CaseId,
    string CompanyName,
    string ReferenceNumber,
    bool IsUrgent) : INotification;
