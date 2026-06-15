using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KYC.Web.Security;

namespace KYC.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager) =>
        _signInManager = signInManager;

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
            ModelState.AddModelError(string.Empty, ErrorMessage);

        ReturnUrl = ReturnUrlNormalizer.Normalize(returnUrl, ReturnUrlNormalizer.DefaultLandingPath);

        if (_signInManager.IsSignedIn(User))
            return LocalRedirect(ReturnUrl);

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        return Page();
    }
}
