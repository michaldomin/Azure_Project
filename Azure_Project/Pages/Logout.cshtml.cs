using Azure_Project.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Azure_Project.Pages;
public class LogoutModel : PageModel
{
    private readonly JwtConfig _jwtConfig;

    public LogoutModel(JwtConfig jwtConfig)
    {
        _jwtConfig = jwtConfig;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            Response.Cookies.Delete(_jwtConfig.TokenCookieName);
        }

        return RedirectToPage("/Index");
    }
}

