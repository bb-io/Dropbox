using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Dropbox.Api;

namespace Apps.Dropbox.Webhooks.Handlers;

public class WebhookHandler : BaseInvocable, IWebhookEventHandler
{
    private const string SubscriptionEvent = "item_updated";
    
    private readonly string _accountId;
    private readonly string _cursorStorageKey;
    private readonly DropboxClient _dropboxClient;

    public WebhookHandler(InvocationContext invocationContext) : base(invocationContext)
    {
        _dropboxClient = DropboxClientFactory.CreateDropboxClient(invocationContext.AuthenticationCredentialsProviders);
        var currentAccount = _dropboxClient.Users.GetCurrentAccountAsync().Result;
    
        if (currentAccount == null) 
            throw new Exception("Could not fetch account details.");
    
        _accountId = currentAccount.AccountId;
        _cursorStorageKey = $"{_accountId}_cursor";
    }
    
    public async Task SubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, 
        Dictionary<string, string> values)
    {
        var bridgeService = new BridgeService(InvocationContext.UriInfo.BridgeServiceUrl.ToString());
        await bridgeService.Subscribe(values["payloadUrl"], _accountId, SubscriptionEvent);
        
        var cursor = await bridgeService.RetrieveValue(_cursorStorageKey);

        if (cursor is null) // Update cursor only if it doesn't exist for this account
        {
            var listFolderResult = await _dropboxClient.Files.ListFolderAsync("", recursive: true);
            cursor = listFolderResult.Cursor;
    
            while (listFolderResult.HasMore)
            {
                listFolderResult = await _dropboxClient.Files.ListFolderContinueAsync(cursor);
                cursor = listFolderResult.Cursor;
            }

            await bridgeService.StoreValue(_cursorStorageKey, cursor);
        }
    }

    public async Task UnsubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders, 
        Dictionary<string, string> values)
    {
        var bridgeService = new BridgeService(InvocationContext.UriInfo.BridgeServiceUrl.ToString());
        var webhooksLeft = await bridgeService.Unsubscribe(values["payloadUrl"], _accountId, SubscriptionEvent);
    
        if (webhooksLeft == 0) // All webhooks for specified accountId rely on a single cursor. That's why cursor can be deleted only if there are no events for the account left
            await bridgeService.DeleteValue(_cursorStorageKey);
    }
}