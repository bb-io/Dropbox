using System.Net;
using Apps.Dropbox.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common.Authentication;
using Dropbox.Api;
using RestSharp;

namespace Apps.Dropbox.Webhooks;

public class BridgeService
{
    private const string SubscriptionEvent = "item_updated";

    private readonly string _accountId;
    private readonly DropboxClient _dropboxClient;

    public BridgeService(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        _dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
        var currentAccount = _dropboxClient.Users.GetCurrentAccountAsync().Result;
        
        if (currentAccount == null) 
            throw new Exception("Could not fetch account details.");
        
        _accountId = currentAccount.AccountId;
    }
    
    public async Task Subscribe(string url)
    {
        var client = new RestClient(ApplicationConstants.BridgeServiceUrl);
        var subscribeRequest = new RestRequest($"/webhooks/{ApplicationConstants.AppName}/{_accountId}/{SubscriptionEvent}", 
            Method.Post);
        subscribeRequest.AddHeader("Blackbird-Token", ApplicationConstants.BlackbirdToken);
        subscribeRequest.AddBody(url);
        await client.ExecuteAsync(subscribeRequest);
        
        var getCursorRequest = new RestRequest($"/storage/{ApplicationConstants.AppName}/{_accountId}_cursor");
        getCursorRequest.AddHeader("Blackbird-Token", ApplicationConstants.BlackbirdToken);
        var getCursorResponse = await client.ExecuteAsync(getCursorRequest);

        if (getCursorResponse.StatusCode == HttpStatusCode.NotFound) // Update cursor only if it doesn't exist for this account
        {
            var listFolderResult = await _dropboxClient.Files.ListFolderAsync("", recursive: true);
            var cursor = listFolderResult.Cursor;

            while (listFolderResult.HasMore)
            {
                listFolderResult = await _dropboxClient.Files.ListFolderContinueAsync(cursor);
                cursor = listFolderResult.Cursor;
            }

            var storeCursorRequest = new RestRequest($"/storage/{ApplicationConstants.AppName}/{_accountId}_cursor",
                Method.Post);
            storeCursorRequest.AddHeader("Blackbird-Token", ApplicationConstants.BlackbirdToken);
            storeCursorRequest.AddBody(cursor);
            await client.ExecuteAsync(storeCursorRequest);
        }
    }

    public async Task Unsubscribe(string url)
    {
        var client = new RestClient(ApplicationConstants.BridgeServiceUrl);
        var getTriggerRequest = new RestRequest($"/webhooks/{ApplicationConstants.AppName}/{_accountId}/{SubscriptionEvent}");
        getTriggerRequest.AddHeader("Blackbird-Token", ApplicationConstants.BlackbirdToken);
        var webhooks = await client.GetAsync<List<BridgeGetResponse>>(getTriggerRequest);
        var webhook = webhooks.FirstOrDefault(w => w.Value == url);
        
        var deleteTriggerRequest = new RestRequest($"/webhooks/{ApplicationConstants.AppName}/{_accountId}/{SubscriptionEvent}/{webhook.Id}", 
            Method.Delete);
        deleteTriggerRequest.AddHeader("Blackbird-Token", ApplicationConstants.BlackbirdToken);
        await client.ExecuteAsync(deleteTriggerRequest);

        if (webhooks.Count == 1) // All webhooks for specified accountId rely on a single cursor
        {                        // That's why cursor can be deleted only if there are no events for the account left
            var deleteCursorRequest = new RestRequest($"/storage/{ApplicationConstants.AppName}/{_accountId}_cursor", 
                Method.Delete);
            deleteCursorRequest.AddHeader("Blackbird-Token", ApplicationConstants.BlackbirdToken);
            await client.ExecuteAsync(deleteCursorRequest);
        }
    }
}