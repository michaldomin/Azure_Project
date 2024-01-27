using Azure_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using Azure_Project.Configuration;

namespace Azure_Project.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        [BindProperty]
        public LoginDto Credentials { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var client = _httpClientFactory.CreateClient();
            var requestUri = $"{_configuration.GetValue<string>("AppSettings:ApiBaseUrl")}/api/user/login";
            var loginResponse = await client.PostAsync(requestUri, new StringContent(JsonSerializer.Serialize(Credentials), Encoding.UTF8, "application/json"));

            if (loginResponse.IsSuccessStatusCode)
            {
                var tokenDto = await JsonSerializer.DeserializeAsync<TokenDto>(await loginResponse.Content.ReadAsStreamAsync());
                var token = tokenDto?.Token;

                var jwtConfig = _configuration.GetSection("JwtConfig").Get<JwtConfig>();
                var tokenCookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(1)
                };

                if (token != null)
                {
                    _httpContextAccessor.HttpContext?.Response.Cookies.Append(jwtConfig.TokenCookieName, token, tokenCookieOptions);
                }

                return RedirectToPage("/Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
        }
    }
}
