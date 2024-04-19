using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.Dropbox.Connections;

public class ConnectionValidator : IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, 
        CancellationToken cancellationToken)
    {
        try
        {
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
            var currentAccount = await dropboxClient.Users.GetCurrentAccountAsync();

            if (currentAccount is null)
            {
                return new ConnectionValidationResponse
                {
                    IsValid = false,
                    Message = "Ping failed"
                };
            }
        
            return new ConnectionValidationResponse
            {
                IsValid = true,
                Message = "Success"
            };
        }
        catch (Exception e)
        {
            return new ConnectionValidationResponse
            {
                IsValid = false,
                Message = e.Message
            };
        }
    }
}