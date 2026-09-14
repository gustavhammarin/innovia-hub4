using System.Net;

namespace Innovia.Api.Tests;

public static class HttpResponseAssertions
{
    public static async Task AssertProblemDetailsAsync(this HttpResponseMessage response, HttpStatusCode expectedStatus)
    {
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(response.StatusCode == expectedStatus, $"Status {response.StatusCode}: {body}");
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
