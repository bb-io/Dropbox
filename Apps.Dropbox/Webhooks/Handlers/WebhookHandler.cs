using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.Dropbox.Webhooks.Handlers;

public class WebhookHandler : IWebhookEventHandler
{
    public async Task SubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, 
        Dictionary<string, string> values)
    {
        var bridge = new BridgeService(authenticationCredentialsProviders);
        await bridge.Subscribe(values["payloadUrl"]);
    }

    public async Task UnsubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, 
        Dictionary<string, string> values)
    {
        var bridge = new BridgeService(authenticationCredentialsProviders);
        await bridge.Unsubscribe(values["payloadUrl"]);
    }
}