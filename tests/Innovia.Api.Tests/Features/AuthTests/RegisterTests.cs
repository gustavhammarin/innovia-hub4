using System.Net;
using System.Net.Http.Json;
using Innovia.Api.Common.Auth.Cookie;
using Microsoft.EntityFrameworkCore;

namespace Innovia.Api.Tests.Features.AuthTests;

[Collection("Database")]
public class RegisterTests : IAsyncLifetime
{
    private readonly DatabaseFixture _dbFixture;
    private ApiFactory _factory = null!;
    private HttpClient _client = null!;

    public RegisterTests(DatabaseFixture dbFixture) => _dbFixture = dbFixture;

    public Task InitializeAsync()
    {
        _factory = new ApiFactory(_dbFixture.ConnectionString);
        _client = _factory.CreateAuthClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Should_Register_And_Set_Cookie_On_Valid_Request()
    {
        var email = $"{Guid.NewGuid()}@test.com";

        var response = await _client.PostAsJsonAsync("/auth/register", new { FirstName = "Test", LastName = "User", Email = email, Password = "Password123!" });
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Status {response.StatusCode}: {body}");
        Assert.Contains(response.Headers, h => h.Key == "Set-Cookie" && h.Value.Any(v => v.Contains(AuthCookieNames.AccessToken)));

        await using var ctx = _dbFixture.CreateDbContext();
        var user = await ctx.Users.SingleAsync(u => u.Email == email);
        var hasRole = await ctx.UserRoles.AnyAsync(r => r.UserId == user.Id);
        Assert.True(hasRole);
    }

    [Fact]
    public async Task Should_Fail_With_Conflict_When_Email_Already_Registered()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/auth/register", new { FirstName = "Test", LastName = "User", Email = email, Password = "Password123!" });

        var second = await _client.PostAsJsonAsync("/auth/register", new { FirstName = "Test", LastName = "User", Email = email, Password = "Password123!" });

        await second.AssertProblemDetailsAsync(HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("Test", "User", "", "Password123!")]
    [InlineData("Test", "User", "not-an-email", "Password123!")]
    [InlineData("Test", "User", "a@b.com", "short")]
    [InlineData("", "User", "a@b.com", "Password123!")]
    [InlineData("Test", "", "a@b.com", "Password123!")]
    public async Task Should_Return_BadRequest_On_Invalid_Input(string firstName, string lastName, string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/auth/register", new { FirstName = firstName, LastName = lastName, Email = email, Password = password });

        await response.AssertProblemDetailsAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Allow_Access_To_Own_Bookings_With_Cookie_Set_After_Register()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/auth/register", new { FirstName = "Test", LastName = "User", Email = email, Password = "Password123!" });

        var response = await _client.GetAsync("/bookings/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Should_Return_ProblemDetails_When_Anonymous_Hits_Protected_Endpoint()
    {
        var response = await _client.GetAsync("/bookings/me");

        await response.AssertProblemDetailsAsync(HttpStatusCode.Unauthorized);
    }
}
