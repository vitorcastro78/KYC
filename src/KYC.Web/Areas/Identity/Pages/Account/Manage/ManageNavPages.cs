using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KYC.Web.Areas.Identity.Pages.Account.Manage;

public static class ManageNavPages
{
    public static string Index => "Index";
    public static string Email => "Email";
    public static string ChangePassword => "ChangePassword";
    public static string DownloadPersonalData => "DownloadPersonalData";
    public static string DeletePersonalData => "DeletePersonalData";
    public static string ExternalLogins => "ExternalLogins";
    public static string TwoFactorAuthentication => "TwoFactorAuthentication";
    public static string PersonalData => "PersonalData";

    public static string? IndexNavClass(ViewContext viewContext) => NavClass(viewContext, Index);
    public static string? EmailNavClass(ViewContext viewContext) => NavClass(viewContext, Email);
    public static string? ChangePasswordNavClass(ViewContext viewContext) => NavClass(viewContext, ChangePassword);
    public static string? DownloadPersonalDataNavClass(ViewContext viewContext) => NavClass(viewContext, DownloadPersonalData);
    public static string? DeletePersonalDataNavClass(ViewContext viewContext) => NavClass(viewContext, DeletePersonalData);
    public static string? ExternalLoginsNavClass(ViewContext viewContext) => NavClass(viewContext, ExternalLogins);
    public static string? TwoFactorAuthenticationNavClass(ViewContext viewContext) => NavClass(viewContext, TwoFactorAuthentication);
    public static string? PersonalDataNavClass(ViewContext viewContext) => NavClass(viewContext, PersonalData);

    private static string? NavClass(ViewContext viewContext, string page)
    {
        var activePage = viewContext.ViewData["ActivePage"] as string
            ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor?.DisplayName ?? string.Empty);
        return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
    }
}
