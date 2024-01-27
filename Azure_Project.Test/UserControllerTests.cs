using Azure_Project.Configuration;
using Azure_Project.Controllers;
using Azure_Project.Data;
using Azure_Project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Azure_Project.Test
{
    public class UserControllerTests
    {
        private DbContextOptions<ApplicationDbContext> options;
        private JwtConfig jwtConfig;
        private UserController controller;

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

            jwtConfig = new JwtConfig
            {
                Secret = "testsecret-testsecret-testsecret-testsecret",
                TokenCookieName = "testtoken"
            };

            var substituteHttpContext = Substitute.For<HttpContext>();
            substituteHttpContext.Response.Cookies.Returns(Substitute.For<IResponseCookies>());

            controller = new UserController(new ApplicationDbContext(options), jwtConfig)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = substituteHttpContext
                }
            };
        }

        [Test]
        public async Task Login_ReturnsOkResult_WhenValidCredentialsProvided()
        {
            var loginUser = new User
            {
                Username = "testuser",
                Password = "testpassword"
            };

            var result = await controller.Login(loginUser) as OkObjectResult;

            Assert.NotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            Assert.NotNull(result.Value);
            Assert.IsInstanceOf<TokenDto>(result.Value);
        }

        [Test]
        public async Task Login_ReturnsUnauthorizedResult_WhenInvalidCredentialsProvided()
        {
            var loginUser = new User
            {
                Username = "testuser",
                Password = "wrongpassword"
            };

            var result = await controller.Login(loginUser) as UnauthorizedObjectResult;

            Assert.NotNull(result);
            Assert.AreEqual(StatusCodes.Status401Unauthorized, result.StatusCode);
            Assert.NotNull(result.Value);
            Assert.AreEqual("{ message = Username or password is incorrect }", result.Value.ToString());
        }

        [Test]
        public async Task Register_ReturnsOkResult_WhenValidUserProvided()
        {
            var newUser = new User
            {
                Username = "newuser",
                Password = "newpassword"
            };

            var result = await controller.Register(newUser) as OkObjectResult;

            Assert.NotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            Assert.NotNull(result.Value);
            Assert.AreEqual("{ message = Registration successful }", result.Value.ToString());
        }

        [Test]
        public async Task Register_ReturnsBadRequestResult_WhenUsernameAlreadyExists()
        {
            var existingUser = new User
            {
                Username = "testuser",
                Password = "newpassword"
            };

            var result = await controller.Register(existingUser) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.NotNull(result.Value);
            Assert.AreEqual("{ message = Username already exists. }", result.Value.ToString());
        }
    }


}