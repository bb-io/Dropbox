﻿using Blackbird.Applications.Sdk.Common.Authentication;
using Dropbox.Api;

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
        return new DropboxClientConfig(applicationName)
        {
            HttpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(20)
            }
        };
    }
}