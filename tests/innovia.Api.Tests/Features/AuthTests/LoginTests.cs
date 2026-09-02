using System.Net;
using System.Net.Http.Json;
using Innovia.Api.Common.Auth.Cookie;

namespace Innovia.Api.Tests.Features.AuthTests;

[Collection("Database")]
public class LoginTests : IAsyncLifetime
{
    private readonly DatabaseFixture _dbFixture;
    private ApiFactory _factory = null!;
    private HttpClient _client = null!;

    public LoginTests(DatabaseFixture dbFixture) => _dbFixture = dbFixture;

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

    private async Task<string> RegisterUserAsync(string email, string password)
    {
        using var registerClient = _factory.CreateAuthClient();
        var response = await registerClient.PostAsJsonAsync("/auth/register", new { Email = email, Password = password });
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Register failed: {response.StatusCode}: {body}");
        return email;
    }

    [Fact]
    public async Task Should_Login_And_Set_Cookie_On_Valid_Credentials()
    {
        var email = await RegisterUserAsync($"{Guid.NewGuid()}@test.com", "Password123!");

        var response = await _client.PostAsJsonAsync("/auth/login", new { Email = email, Password = "Password123!" });
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Status {response.StatusCode}: {body}");
        Assert.Contains(response.Headers, h => h.Key == "Set-Cookie" && h.Value.Any(v => v.Contains(AuthCookieNames.AccessToken)));
    }

    [Fact]
    public async Task Should_Fail_With_Forbidden_When_Password_Is_Wrong()
    {
        var email = await RegisterUserAsync($"{Guid.NewGuid()}@test.com", "Password123!");

        var response = await _client.PostAsJsonAsync("/auth/login", new { Email = email, Password = "WrongPassword123!" });

        await response.AssertProblemDetailsAsync(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Should_Fail_With_Forbidden_When_Email_Does_Not_Exist()
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new { Email = $"{Guid.NewGuid()}@test.com", Password = "Password123!" });

        await response.AssertProblemDetailsAsync(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("", "Password123!")]
    [InlineData("a@b.com", "")]
    public async Task Should_Return_BadRequest_On_Invalid_Input(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new { Email = email, Password = password });

        await response.AssertProblemDetailsAsync(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Allow_Access_To_Own_Bookings_With_Cookie_Set_After_Login()
    {
        var email = await RegisterUserAsync($"{Guid.NewGuid()}@test.com", "Password123!");

        await using var ctx = _dbFixture.CreateDbContext();
        var userId = ctx.Users.Single(u => u.Email == email).Id;

        await _client.PostAsJsonAsync("/auth/login", new { Email = email, Password = "Password123!" });
        var response = await _client.GetAsync($"/bookings/user/{userId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
