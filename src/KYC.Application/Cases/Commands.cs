using KYC.Application.Interfaces;
using KYC.Domain.Enums;
using KYC.Domain.ValueObjects;
using MediatR;

namespace KYC.Application.Cases;

public record StartKycCaseCommand(
    string Nif,
    string RequestedBy,
    CreditAmount RequestedAmount,
    RelationshipType? RelationshipType = null,
    string? CaeCode = null) : IRequest<Guid>;

public record ApproveKycCaseCommand(Guid CaseId, string AnalystId, string Notes, string? SecondApproverId = null) : IRequest<Unit>;

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

public record InitiateEntityVerificationCommand(
    Guid CaseId, Guid PartyId, IdentityVerificationMethod Method, string RequestedBy) : IRequest<IdentityVerificationSession>;

public record RecordPresentialVerificationCommand(
    Guid CaseId, Guid PartyId, string AnalystId, string DocumentReference) : IRequest<Unit>;

public record RecordVerificationResultCommand(
    Guid PartyId,
    string SessionId,
    bool IsVerified,
    string? FailureReason,
    string? EidasLevel) : IRequest<Unit>;

public record ReportRcbeDiscrepancyCommand(Guid CaseId, Guid PartyId, string AnalystId) : IRequest<Unit>;

public record SubmitSarCommand(
    Guid CaseId, string SuspicionDescription, string AnalystId, bool IsUrgent) : IRequest<UifSubmissionResult>;

/// <summary>Regista referência UIF obtida fora da API (processo manual homologado).</summary>
public record RegisterManualUifReferenceCommand(
    Guid CaseId,
    string ReferenceNumber,
    string AnalystId) : IRequest<Unit>;

public record MarkSarNotRequiredCommand(Guid CaseId, string AnalystId, string Justification) : IRequest<Unit>;

public record SetFundsOriginCommand(Guid CaseId, string Description, bool Verified, string? DocumentId) : IRequest<Unit>;

public record TriggerPeriodicReviewCommand(Guid CaseId, string InitiatedBy, string? ReviewNotes) : IRequest<Unit>;

public record EscalateToSupervisorCommand(Guid CaseId, string Reason) : IRequest<Unit>;

public record GenerateAmlReportCommand(int Year, string RequestedBy) : IRequest<Guid>;

public record SubmitAmlReportToBdpCommand(Guid ReportId, string SubmittedBy) : IRequest<string>;

public record CreateScoringEngineConfigCommand(
    string Version,
    string LocalModelName,
    string SystemPromptHash,
    string ApprovedBy) : IRequest<Guid>;

public record CreateDpiaRecordCommand(string Version, string DocumentPath, string ApprovedBy) : IRequest<Guid>;

public record UploadDpiaDocumentCommand(string Version, Stream Content, string FileName) : IRequest<string>;

public record CreateCustomerAcceptancePolicyCommand(string Version, string ApprovedBy) : IRequest<Guid>;
