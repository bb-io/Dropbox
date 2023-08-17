using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Microsoft.AspNetCore.WebUtilities;

namespace Apps.Dropbox.Auth.OAuth2;

public class OAuth2AuthorizeService : IOAuth2AuthorizeService
{
    public string GetAuthorizationUrl(Dictionary<string, string> values)
    {
        const string oauthUrl = "https://www.dropbox.com/oauth2/authorize";
        var parameters = new Dictionary<string, string>
        {
            { "client_id", ApplicationConstants.ClientId },
            { "redirect_uri", ApplicationConstants.RedirectUri },
            { "state", values["state"] },
            { "response_type", "code" },
            { "token_access_type", "offline" },
            { "prompt", "consent" }
        };
        return QueryHelpers.AddQueryString(oauthUrl, parameters);
    }
}