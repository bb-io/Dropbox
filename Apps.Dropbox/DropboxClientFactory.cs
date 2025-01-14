using System;
using System.Net;
using Blackbird.Applications.Sdk.Common.Authentication;
using Dropbox.Api;
using Polly;
using Polly.Extensions.Http;

namespace Apps.Dropbox;

public static class DropboxClientFactory
{
    public static DropboxClient CreateDropboxClient(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var accessToken = authenticationCredentialsProviders.First(p => p.KeyName == "Access token").Value;
        var config = GetConfig(ApplicationConstants.ApplicationName);
        return new DropboxClient(accessToken, config);
    }

    public static DropboxTeamClient CreateDropboxTeamClient(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var accessToken = authenticationCredentialsProviders.First(p => p.KeyName == "Access token").Value;
        var config = GetConfig(ApplicationConstants.ApplicationName);
        return new DropboxTeamClient(accessToken, config);
    }

    private static DropboxClientConfig GetConfig(string applicationName)
    {
        var retryPolicy = Policy.Handle<HttpRequestException>()
             .OrResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.TooManyRequests)
             .WaitAndRetryAsync(
            retryCount: 5,
            sleepDurationProvider: (attempt, outcome, ctx) =>
            {
                var response = outcome.Result;
                if (response != null && response.Headers.TryGetValues("Retry-After", out var values))
                {
                    var retryAfterValue = values.FirstOrDefault();
                    if (double.TryParse(retryAfterValue, out double seconds))
                    {
                        return TimeSpan.FromSeconds(seconds);
                    }
                }

                return TimeSpan.FromSeconds(5);
            },
            onRetryAsync: async (outcome, timeSpan, attempt, ctx) =>{});

        var pollyHandler = new PollyHandler(retryPolicy)
        {
            InnerHandler = new HttpClientHandler()
        };

        var httpClient = new HttpClient(pollyHandler)
        {
            Timeout = TimeSpan.FromMinutes(20)
        };

        return new DropboxClientConfig(applicationName)
        {
            HttpClient = httpClient
        };
    }
}