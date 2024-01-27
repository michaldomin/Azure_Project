using Azure_Project.Data;
using Azure_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Azure_Project.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PostsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PostsController> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public PostsController(ApplicationDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<PostsController> logger, IServiceScopeFactory scopeFactory)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPosts()
    {
        var posts = await _context.Posts
            .Include(p => p.User)
            .OrderByDescending(p => p.CreationDate)
            .Take(20)
            .Select(p => new PostDto
            {
                Id = p.Id,
                User = new UserDto
                {
                    Id = p.User.Id,
                    Username = p.User.Username,
                },
                CreationDate = p.CreationDate,
                Text = p.Text,
                Polarity = p.Polarity,
                Subjectivity = p.Subjectivity
            })
            .ToListAsync();

        return Ok(posts);
    }


    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreatePost(TextData text)
    {
        if (string.IsNullOrWhiteSpace(text.Text))
        {
            return BadRequest("Text is required.");
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized("User not recognized.");
        }
        var post = new Post
        {
            UserId = int.Parse(userId),
            Text = text.Text,
            CreationDate = DateTime.UtcNow,
            Polarity = null,
            Subjectivity = null
        };

        post = _context.Posts.Add(post).Entity;
        await _context.SaveChangesAsync();

        GetAndUpdateSentimentAnalysis(post, text);

        return CreatedAtAction(nameof(GetAllPosts), new { id = post.Id }, post);
    }

    private void GetAndUpdateSentimentAnalysis(Post post, TextData text)
    {
        var requestUri = _configuration.GetValue<string>("SentimentAnalysis:FunctionUri");

        var sentimentRequestJson = JsonSerializer.Serialize(text);
        var content = new StringContent(sentimentRequestJson, Encoding.UTF8, "application/json");

        _httpClient.PostAsync(requestUri, content).ContinueWith(async responseTask =>
        {
            var response = await responseTask;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var sentimentResult = JsonSerializer.Deserialize<SentimentResult>(responseContent);

                if (sentimentResult != null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var scopedContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var scopedPost = await scopedContext.Posts.FindAsync(post.Id);

                    if (scopedPost != null)
                    {
                        scopedPost.Polarity = sentimentResult.Polarity;
                        scopedPost.Subjectivity = sentimentResult.Subjectivity;

                        scopedContext.Posts.Update(scopedPost);
                        await scopedContext.SaveChangesAsync().ConfigureAwait(false);
                    }
                }
            }
        });
    }

}
