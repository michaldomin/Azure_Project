using System.Security.Claims;
using Azure_Project.Configuration;
using Azure_Project.Controllers;
using Azure_Project.Data;
using Azure_Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Azure_Project.Test;

public class PostControllerTests
{
    private DbContextOptions<ApplicationDbContext> options;
    private PostsController controller;

    [SetUp]
    public void Setup()
    {
        var testName = TestContext.CurrentContext.Test.Name;
        options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: testName)
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Password = BCrypt.Net.BCrypt.HashPassword("testpassword")
            };

            context.Users.Add(user);
            context.SaveChanges();
        }

        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var httpClient = Substitute.For<HttpClient>();
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);

        controller = new PostsController(new ApplicationDbContext(options), httpClientFactory, Substitute.For<IConfiguration>(), Substitute.For<ILogger<PostsController>>(), Substitute.For<IServiceScopeFactory>());
        var claimsIdentity = new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        });
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
    }

    [Test]
    public async Task CreatePost_ReturnsBadRequestResult_WhenTextIsNullOrWhiteSpace()
    {
        var textData = new TextData
        {
            Text = ""
        };

        var result = await controller.CreatePost(textData) as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.AreEqual("Text is required.", result.Value);
    }

    [Test]
    public async Task CreatePost_ReturnsUnauthorizedResult_WhenUserNotRecognized()
    {
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();

        var textData = new TextData
        {
            Text = "Test post"
        };

        var result = await controller.CreatePost(textData) as UnauthorizedObjectResult;

        Assert.NotNull(result);
        Assert.AreEqual(StatusCodes.Status401Unauthorized, result.StatusCode);
        Assert.AreEqual("User not recognized.", result.Value);
    }

    [Test]
    public async Task CreatePost_ReturnsCreatedAtActionResult_WhenValidDataProvided()
    {
        var textData = new TextData
        {
            Text = "Test post"
        };

        var result = await controller.CreatePost(textData) as CreatedAtActionResult;

        Assert.NotNull(result);
        Assert.AreEqual(StatusCodes.Status201Created, result.StatusCode);
        Assert.AreEqual(nameof(PostsController.GetAllPosts), result.ActionName);
        Assert.NotNull(result.Value);
        Assert.IsInstanceOf<Post>(result.Value);
    }
    [Test]
    public async Task GetAllPosts_ReturnsOkResult_WithListOfPosts()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "GetAllPostsTest")
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Password = BCrypt.Net.BCrypt.HashPassword("testpassword")
            };

            var postsInit = new List<Post>
            {
                new Post
                {
                    Id = 1,
                    User = user,
                    CreationDate = DateTime.UtcNow,
                    Text = "Test post 1",
                    Polarity = 0.5,
                    Subjectivity = 0.8
                },
                new Post
                {
                    Id = 2,
                    User = user,
                    CreationDate = DateTime.UtcNow,
                    Text = "Test post 2",
                    Polarity = 0.2,
                    Subjectivity = 0.6
                }
            };

            context.Users.Add(user);
            context.Posts.AddRange(postsInit);
            context.SaveChanges();
        }

        var controller = new PostsController(new ApplicationDbContext(options), Substitute.For<IHttpClientFactory>(), Substitute.For<IConfiguration>(), Substitute.For<ILogger<PostsController>>(), Substitute.For<IServiceScopeFactory>());

        // Act
        var result = await controller.GetAllPosts() as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
        Assert.NotNull(result.Value);
        Assert.IsInstanceOf<List<PostDto>>(result.Value);

        var posts = result.Value as List<PostDto>;
        Assert.AreEqual(2, posts.Count);
        Assert.AreEqual(2, posts[0].Id);
        Assert.AreEqual("testuser", posts[0].User.Username);
        Assert.AreEqual("Test post 2", posts[0].Text);
        Assert.AreEqual(0.2, posts[0].Polarity);
        Assert.AreEqual(0.6, posts[0].Subjectivity);
        Assert.AreEqual(1, posts[1].Id);
        Assert.AreEqual("testuser", posts[1].User.Username);
        Assert.AreEqual("Test post 1", posts[1].Text);
        Assert.AreEqual(0.5, posts[1].Polarity);
        Assert.AreEqual(0.8, posts[1].Subjectivity);
    }


}