using System.Text.Json;
using KYC.Application.Cases;
using KYC.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KYC.Web.Endpoints;

public static class IdentityWebhookEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static IEndpointRouteBuilder MapIdentityWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/identity/webhook", async (
            HttpRequest request,
            IMediator mediator,
            IConfiguration configuration,
            CancellationToken ct) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync(ct);

            var secret = configuration["IdentityVerification:WebhookSecret"];
            var signature = request.Headers["X-Webhook-Signature"].ToString();
            if (!WebhookSignatureValidator.Validate(body, signature, secret))
                return Results.Unauthorized();

            IdentityWebhookPayload? payload;
            try
            {
                payload = JsonSerializer.Deserialize<IdentityWebhookPayload>(body, JsonOptions);
            }
            catch
            {
                return Results.BadRequest("JSON inválido.");
            }

            if (payload is null || payload.PartyId == Guid.Empty || string.IsNullOrWhiteSpace(payload.SessionId))
                return Results.BadRequest("partyId e sessionId obrigatórios.");

            await mediator.Send(new RecordVerificationResultCommand(
                payload.PartyId,
                payload.SessionId,
                payload.Verified,
                payload.FailureReason,
                payload.EidasLevel,
                payload.LivenessScore), ct);

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
    string? EidasLevel,
    string? LivenessScore = null);
