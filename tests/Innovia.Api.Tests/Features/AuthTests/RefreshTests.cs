using System.Net;
using System.Net.Http.Json;
using Innovia.Api.Common.Auth.Cookie;

namespace Innovia.Api.Tests.Features.AuthTests;

[Collection("Database")]
public class RefreshTests : IAsyncLifetime
{
    private readonly DatabaseFixture _dbFixture;
    private ApiFactory _factory = null!;
    private HttpClient _client = null!;

    public RefreshTests(DatabaseFixture dbFixture) => _dbFixture = dbFixture;

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

    private async Task<HttpResponseMessage> RegisterAndLoginAsync(string email, string password)
    {
        var register = await _client.PostAsJsonAsync("/auth/register", new { FirstName = "Test", LastName = "User", Email = email, Password = password });
        Assert.True(register.StatusCode == HttpStatusCode.OK, $"Register failed: {register.StatusCode}");

        var login = await _client.PostAsJsonAsync("/auth/login", new { FirstName = "Test", LastName = "User", Email = email, Password = password });
        Assert.True(login.StatusCode == HttpStatusCode.OK, $"Login failed: {login.StatusCode}");
        return login;
    }

    private static string ExtractCookieValue(HttpResponseMessage response, string cookieName) =>
        response.Headers.GetValues("Set-Cookie")
            .First(v => v.StartsWith($"{cookieName}="))
            .Split(';')[0][(cookieName.Length + 1)..];

    private HttpRequestMessage BuildRefreshRequest(string rawRefreshToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/auth/refresh");
        request.Headers.Add("Cookie", $"{AuthCookieNames.RefreshToken}={rawRefreshToken}");
        return request;
    }

    [Fact]
    public async Task Should_Issue_New_Tokens_On_Valid_Refresh_Token()
    {
        await RegisterAndLoginAsync($"{Guid.NewGuid()}@test.com", "Password123!");

        var response = await _client.PostAsync("/auth/refresh", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(response.Headers, h => h.Key == "Set-Cookie" && h.Value.Any(v => v.Contains(AuthCookieNames.AccessToken)));
        Assert.Contains(response.Headers, h => h.Key == "Set-Cookie" && h.Value.Any(v => v.Contains(AuthCookieNames.RefreshToken)));
    }

    [Fact]
    public async Task Should_Fail_When_No_Refresh_Cookie_Present()
    {
        var response = await _client.PostAsync("/auth/refresh", null);

        await response.AssertProblemDetailsAsync(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Should_Fail_When_Refresh_Token_Is_Garbage()
    {
        using var rawClient = _factory.CreateClient();
        var response = await rawClient.SendAsync(BuildRefreshRequest("not-a-real-token"));

        await response.AssertProblemDetailsAsync(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Should_Allow_Access_To_Protected_Endpoint_With_New_Access_Token_After_Refresh()
    {
        await RegisterAndLoginAsync($"{Guid.NewGuid()}@test.com", "Password123!");

        await _client.PostAsync("/auth/refresh", null);
        var response = await _client.GetAsync("/bookings/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Should_Reject_Old_Refresh_Token_After_Rotation()
    {
        var login = await RegisterAndLoginAsync($"{Guid.NewGuid()}@test.com", "Password123!");
        var oldRefreshToken = ExtractCookieValue(login, AuthCookieNames.RefreshToken);

        await _client.PostAsync("/auth/refresh", null);

        using var rawClient = _factory.CreateClient();
        var response = await rawClient.SendAsync(BuildRefreshRequest(oldRefreshToken));

        await response.AssertProblemDetailsAsync(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Should_Revoke_All_Tokens_When_Already_Revoked_Token_Is_Reused()
    {
        var login = await RegisterAndLoginAsync($"{Guid.NewGuid()}@test.com", "Password123!");
        var firstRefreshToken = ExtractCookieValue(login, AuthCookieNames.RefreshToken);

        var rotated = await _client.PostAsync("/auth/refresh", null);
        var secondRefreshToken = ExtractCookieValue(rotated, AuthCookieNames.RefreshToken);

        using var reuseClient = _factory.CreateClient();
        var reuseResponse = await reuseClient.SendAsync(BuildRefreshRequest(firstRefreshToken));
        await reuseResponse.AssertProblemDetailsAsync(HttpStatusCode.Forbidden);

        using var secondClient = _factory.CreateClient();
        var secondResponse = await secondClient.SendAsync(BuildRefreshRequest(secondRefreshToken));
        await secondResponse.AssertProblemDetailsAsync(HttpStatusCode.Forbidden);
    }
}
