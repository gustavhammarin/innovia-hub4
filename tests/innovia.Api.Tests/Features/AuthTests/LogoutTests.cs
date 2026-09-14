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
        var response = await _client.PostAsJsonAsync("/auth/register", new { FirstName = "Test", LastName = "User", Email = email, Password = password });
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
    public async Task Should_Return_Ok_When_Not_Authenticated()
    {
        var response = await _client.PostAsync("/auth/logout", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Should_Revoke_Refresh_Token_In_Database_On_Logout()
    {
        var email = $"{Guid.NewGuid()}@test.com";
        await RegisterAsync(email, "Password123!");

        var login = await _client.PostAsJsonAsync("/auth/login", new { Email = email, Password = "Password123!" });
        var refreshToken = login.Headers.GetValues("Set-Cookie")
            .First(v => v.StartsWith($"{AuthCookieNames.RefreshToken}="))
            .Split(';')[0][(AuthCookieNames.RefreshToken.Length + 1)..];

        await _client.PostAsync("/auth/logout", null);

        using var rawClient = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "/auth/refresh");
        request.Headers.Add("Cookie", $"{AuthCookieNames.RefreshToken}={refreshToken}");
        var response = await rawClient.SendAsync(request);

        await response.AssertProblemDetailsAsync(HttpStatusCode.Forbidden);
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
