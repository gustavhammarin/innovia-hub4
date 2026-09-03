using System.Net;
using System.Net.Http.Json;
using Innovia.Api.Common.Auth.Cookie;

namespace Innovia.Api.Tests.Features.AuthTests;

[Collection("Database")]
public class LogoutTests : IAsyncLifetime
{
    private readonly DatabaseFixture _dbFixture;
    private ApiFactory _factory = null!;
    private HttpClient _client = null!;

    public LogoutTests(DatabaseFixture dbFixture) => _dbFixture = dbFixture;

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

    private async Task RegisterAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/auth/register", new { Email = email, Password = password });
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Register failed: {response.StatusCode}: {body}");
    }

    [Fact]
    public async Task Should_Clear_Cookie_When_Authenticated()
    {
        await RegisterAsync($"{Guid.NewGuid()}@test.com", "Password123!");

        var response = await _client.PostAsync("/auth/logout", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(response.Headers, h => h.Key == "Set-Cookie"
            && h.Value.Any(v => v.Contains(AuthCookieNames.AccessToken) && v.Contains("expires=")));
    }

    [Fact]
    public async Task Should_Return_ProblemDetails_When_Not_Authenticated()
    {
        var response = await _client.PostAsync("/auth/logout", null);

        await response.AssertProblemDetailsAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Deny_Protected_Endpoint_After_Logout()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        await RegisterAsync(email, "Password123!");

        var beforeLogout = await _client.GetAsync("/bookings/me");
        Assert.Equal(HttpStatusCode.OK, beforeLogout.StatusCode);

        await _client.PostAsync("/auth/logout", null);

        var afterLogout = await _client.GetAsync("/bookings/me");
        await afterLogout.AssertProblemDetailsAsync(HttpStatusCode.Unauthorized);
    }
}
