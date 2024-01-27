using System.Text;
using System.Text.Json;
using Azure_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class RegisterModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public RegisterModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [BindProperty]
    public User NewUser { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var client = _httpClientFactory.CreateClient();
        var requestUri = $"{_configuration.GetValue<string>("AppSettings:ApiBaseUrl")}/api/user/register";
        var registerResponse = await client.PostAsync(requestUri, new StringContent(JsonSerializer.Serialize(NewUser), Encoding.UTF8, "application/json"));

        if (registerResponse.IsSuccessStatusCode)
        {
            return RedirectToPage("/Login");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Registration failed.");
            return Page();
        }
    }
}