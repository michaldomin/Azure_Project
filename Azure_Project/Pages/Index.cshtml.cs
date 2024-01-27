using System.Net.Http.Headers;
using Azure_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Azure_Project.Configuration;

namespace Azure_Project.Pages
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory; // Added field
        private readonly JwtConfig _jwtConfig;

        public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, JwtConfig jwtConfig)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _jwtConfig = jwtConfig;
            _httpClientFactory = httpClientFactory;
            Posts = [];
        }

        public List<Post> Posts { get; set; }

        public async Task OnGetAsync()
        {
            var requestUri = $"{_configuration.GetValue<string>("AppSettings:ApiBaseUrl")}/api/posts";
            var response = await _httpClient.GetAsync(requestUri);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var posts = JsonSerializer.Deserialize<List<Post>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                Posts = posts ?? new List<Post>();
            }
        }

        public async Task<IActionResult> OnPostCreatePostAsync(string text)
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(text))
            {
                return Page();
            }

            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                return Unauthorized();
            }

            var newPost = new Post
            {
                UserId = userId,
                Text = text,
                CreationDate = DateTime.UtcNow
            };

            var client = _httpClientFactory.CreateClient();
            var requestUri = $"{_configuration.GetValue<string>("AppSettings:ApiBaseUrl")}/api/posts";
            var token = _httpContextAccessor.HttpContext?.Request.Cookies[_jwtConfig.TokenCookieName];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.PostAsync(requestUri, new StringContent(JsonSerializer.Serialize(newPost), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage();
            }

            return Page();
        }
    }
}
