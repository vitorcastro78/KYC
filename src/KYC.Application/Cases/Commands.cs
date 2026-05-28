using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;
using MediatR;

namespace KYC.Application.Cases;

public record StartKycCaseCommand(string Nif, string RequestedBy, CreditAmount RequestedAmount) : IRequest<Guid>;

public record ApproveKycCaseCommand(Guid CaseId, string AnalystId, string Notes) : IRequest<Unit>;

public record RejectKycCaseCommand(Guid CaseId, string AnalystId, string Reason) : IRequest<Unit>;

public record RequestManualReviewCommand(Guid CaseId, string Reason) : IRequest<Unit>;

public record OverrideSignalCommand(Guid SignalId, string AnalystId, bool Confirm, string Notes) : IRequest<Unit>;

public record AddManualCasePartyCommand(
    Guid CaseId,
    string ActorId,
    string Name,
    string? Nif,
    EntityType Type,
    EntityRole Role,
    decimal OwnershipPercentage,
    int UboDepthLevel,
    Guid? ParentPartyId,
    string? Nationality,
    bool RunScreeningAfterAdd) : IRequest<Guid>;

public record ScreenCasePartyCommand(Guid CaseId, Guid PartyId, string ActorId) : IRequest<Unit>;

public record UploadCaseDocumentCommand(
    Guid CaseId,
    string ActorId,
    string FileName,
    string ContentType,
    Stream Content,
    CaseDocumentKind Kind,
    Guid? CasePartyId) : IRequest<Guid>;

public record RerunKycCaseScreeningCommand(Guid CaseId, string ActorId) : IRequest<Unit>;
