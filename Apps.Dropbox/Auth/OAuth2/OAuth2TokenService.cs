using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Blackbird.Applications.Sdk.Common.Invocation;
using System.Globalization;
using System.Text.Json;

namespace Apps.Dropbox.Auth.OAuth2;

public class OAuth2TokenService(InvocationContext invocationContext) 
    : BaseInvocable(invocationContext), IOAuth2TokenService, ITokenRefreshable
{
    private const string TokenUrl = "https://api.dropbox.com/oauth2/token";
    private const string ExpiresAtKeyName = "expires_at";

    public bool IsRefreshToken(Dictionary<string, string> values) 
        => values.TryGetValue(ExpiresAtKeyName, out var expireValue) && DateTime.UtcNow > DateTime.Parse(expireValue);

    // Dropbox's access token expires in 4 hours
    public int? GetRefreshTokenExprireInMinutes(Dictionary<string, string> values)
    {
        if (!values.TryGetValue(ExpiresAtKeyName, out var expireValue))
            return null;

        if (!DateTime.TryParse(expireValue, out var expireDate))
            return null;

        var difference = expireDate - DateTime.UtcNow;

        return (int)difference.TotalMinutes - 20;
    }

    public async Task<Dictionary<string, string>> RefreshToken(Dictionary<string, string> values, 
        CancellationToken cancellationToken) 
    { 
        const string grantType = "refresh_token";
        var bodyParameters = new Dictionary<string, string>
        {
            { "grant_type", grantType },
            { "refresh_token", values["refresh_token"] },
            { "client_id", ApplicationConstants.ClientId },
            { "client_secret", ApplicationConstants.ClientSecret }
        };
        return await RequestToken(bodyParameters, cancellationToken);
    }
    
    public async Task<Dictionary<string, string>> RequestToken(
        string state, 
        string code, 
        Dictionary<string, string> values, 
        CancellationToken cancellationToken)
    { 
        const string grantType = "authorization_code"; 
        var bodyParameters = new Dictionary<string, string> 
        { 
            { "code", code },
            { "grant_type", grantType },
            { "client_id", ApplicationConstants.ClientId }, 
            { "client_secret", ApplicationConstants.ClientSecret },
            { "redirect_uri", $"{InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/AuthorizationCode" }
        };
        return await RequestToken(bodyParameters, cancellationToken);
    }

    public Task RevokeToken(Dictionary<string, string> values)
    { 
        throw new NotImplementedException();
    }
    
    private async Task<Dictionary<string, string>> RequestToken(Dictionary<string, string> bodyParameters, 
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        using HttpClient httpClient = new HttpClient(); 
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        using var httpContent = new FormUrlEncodedContent(bodyParameters); 
        using var response = await httpClient.PostAsync(TokenUrl, httpContent, cancellationToken); 
        var bodyParametersString = string.Join(", ", bodyParameters.Select(p => $"{p.Key}: {p.Value}"));
        
        if (!response.IsSuccessStatusCode) 
        { 
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken); 
            InvocationContext.Logger?.LogError($"[Dropbox OAuth] Failed to request token. Status code: {response.StatusCode}; Content: {errorContent}.", new object []{ bodyParameters });
            throw new InvalidOperationException($"Failed to request token: {errorContent}; Body: {bodyParametersString}"); 
        }
        
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken); 
        var resultDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent)?
                                   .ToDictionary(r => r.Key, r => r.Value?.ToString()) 
                               ?? throw new InvalidOperationException($"Invalid response content: {responseContent}");
        var expiresIn = int.Parse(resultDictionary["expires_in"] ?? throw new InvalidOperationException($"Missing expires_in value. Response: {responseContent}"));
        var expiresAt = utcNow.AddSeconds(expiresIn);
        
        InvocationContext.Logger?.LogError(
            $"[Dropbox OAuth] Token request succeeded, grant: {bodyParameters.GetValueOrDefault("grant_type")}. " +
            $"expires_in: {expiresIn}, expires_at: {expiresAt}, " +
            $"refresh_token returned: {resultDictionary.ContainsKey("refresh_token")}",
            []);
        
        resultDictionary.Add(ExpiresAtKeyName, expiresAt.ToString(CultureInfo.InvariantCulture));
        return resultDictionary;
    }
}