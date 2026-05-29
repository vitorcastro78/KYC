using KYC.Application.Cases;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KYC.Web.Endpoints;

public static class IdentityWebhookEndpoints
{
    public static IEndpointRouteBuilder MapIdentityWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/webhook", async (IdentityWebhookPayload payload, IMediator mediator, CancellationToken ct) =>
        {
            if (payload.PartyId == Guid.Empty || string.IsNullOrWhiteSpace(payload.SessionId))
                return Results.BadRequest("partyId e sessionId obrigatórios.");

            await mediator.Send(new RecordVerificationResultCommand(
                payload.PartyId,
                payload.SessionId,
                payload.Verified,
                payload.FailureReason,
                payload.EidasLevel), ct);

            return Results.Ok();
        }).AllowAnonymous();

        return app;
    }
}

public sealed record IdentityWebhookPayload(
    Guid PartyId,
    string SessionId,
    bool Verified,
    string? FailureReason,
    string? EidasLevel);
