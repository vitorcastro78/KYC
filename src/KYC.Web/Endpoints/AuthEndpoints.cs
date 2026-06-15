using KYC.Web.Security;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;

namespace KYC.Web.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/account/login", LoginAsync)
            .AllowAnonymous()
            .DisableAntiforgery();
    }

    private static async Task<IResult> LoginAsync(
        HttpContext context,
        IConfiguration configuration,
        SignInManager<ApplicationUser> signInManager,
        ILoggerFactory loggerFactory)
    {
        if (!FinSightEmbedCookies.IsEmbedMode(configuration))
        {
            var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
            await antiforgery.ValidateRequestAsync(context);
        }

        var form = await context.Request.ReadFormAsync();
        var email = form["email"].ToString();
        var password = form["password"].ToString();
        var rememberMe = form.TryGetValue("rememberMe", out var remember) &&
                         (remember == "true" || remember == "on");
        var returnUrl = form["returnUrl"].ToString();

        var target = ReturnUrlNormalizer.Normalize(returnUrl);
        var result = await signInManager.PasswordSignInAsync(
            email, password, rememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            loggerFactory.CreateLogger("KYC.Auth")
                .LogInformation("Utilizador {Email} autenticado.", email);
            return Results.Redirect(target);
        }

        if (result.IsLockedOut)
            return Results.Redirect($"/Identity/Account/Login?returnUrl={Uri.EscapeDataString(target)}&error=lockedout");

        return Results.Redirect($"/Identity/Account/Login?returnUrl={Uri.EscapeDataString(target)}&error=invalid");
    }
}
