using System.Net;
using Apps.Dropbox.Dtos;
using Apps.Dropbox.Webhooks.Handlers;
using Apps.Dropbox.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Dropbox.Api.Files;
using RestSharp;

namespace Apps.Dropbox.Webhooks;

[WebhookList]
public class WebhookList : BaseInvocable
{
    private readonly List<Metadata> _changedItems;
    private readonly string _cursor;
    private IEnumerable<AuthenticationCredentialsProvider> Creds =>
        InvocationContext.AuthenticationCredentialsProviders;

    public WebhookList(InvocationContext invocationContext) : base(invocationContext)
    {
        _changedItems = GetChangedItems(out _cursor);
    }
    
    [Webhook("On files created or updated", typeof(WebhookHandler), 
        Description = "This webhook is triggered when a file or files are created or updated.")]
    public async Task<WebhookResponse<ListResponse<FileDto>>> FileCreatedOrUpdated(WebhookRequest request)
    {
        var files = _changedItems.Where(item => item.IsFile);
        
        if (!files.Any()) 
            return new WebhookResponse<ListResponse<FileDto>>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK), 
                ReceivedWebhookRequestType = WebhookRequestType.Preflight
            };
        
        await StoreCursor(_cursor);
        return new WebhookResponse<ListResponse<FileDto>>
        {
            HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
            Result = new ListResponse<FileDto> { Items = files.Select(file => new FileDto(file.AsFile)) }
        };
    }
    
    [Webhook("On folders created or updated", typeof(WebhookHandler), 
        Description = "This webhook is triggered when a folder or folders are created or updated.")]
    public async Task<WebhookResponse<ListResponse<FolderDto>>> FolderCreatedOrUpdated(WebhookRequest request)
    {
        var folders = _changedItems.Where(item => item.IsFolder);
        
        if (!folders.Any()) 
            return new WebhookResponse<ListResponse<FolderDto>>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK), 
                ReceivedWebhookRequestType = WebhookRequestType.Preflight
            };

        await StoreCursor(_cursor);
        return new WebhookResponse<ListResponse<FolderDto>>
        {
            HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
            Result = new ListResponse<FolderDto> { Items = folders.Select(folder => new FolderDto(folder.AsFolder)) }
        };
    }
    
    [Webhook("On files or folders deleted", typeof(WebhookHandler), 
        Description = "This webhook is triggered when file(s) or folder(s) are deleted.")]
    public async Task<WebhookResponse<ListResponse<DeletedItemDto>>> FileOrFolderDeleted(WebhookRequest request)
    {
        var deletedItems = _changedItems.Where(item => item.IsDeleted);
        
        if (!deletedItems.Any()) 
            return new WebhookResponse<ListResponse<DeletedItemDto>>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK), 
                ReceivedWebhookRequestType = WebhookRequestType.Preflight
            };
        
        await StoreCursor(_cursor);
        return new WebhookResponse<ListResponse<DeletedItemDto>>
        {
            HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
            Result = new ListResponse<DeletedItemDto> 
                { Items = deletedItems.Select(item => new DeletedItemDto(item.AsDeleted)) }
        };
    }

    private List<Metadata> GetChangedItems(out string cursor)
    {
        var accountId = GetAccountId().Result;
        var bridgeClient = new RestClient(new RestClientOptions { BaseUrl = new Uri(ApplicationConstants.BridgeServiceUrl) });
        var getCursorRequest = new RestRequest($"/storage/{ApplicationConstants.AppName}/{accountId}_cursor");
        getCursorRequest.AddHeader("Blackbird-Token", ApplicationConstants.BlackbirdToken);
        var getCursorResponse = bridgeClient.Execute(getCursorRequest);
        cursor = getCursorResponse.Content.Trim('"');

        var dropboxClient = DropboxClientFactory.CreateDropboxClient(Creds);
        var changedItems = new List<Metadata>();
        var listFolderResult = dropboxClient.Files.ListFolderContinueAsync(cursor).Result;
        cursor = listFolderResult.Cursor;
        changedItems.AddRange(listFolderResult.Entries);

        while (listFolderResult.HasMore)
        {
            listFolderResult = dropboxClient.Files.ListFolderContinueAsync(cursor).Result;
            cursor = listFolderResult.Cursor;
            changedItems.AddRange(listFolderResult.Entries);
        }

        return changedItems;
    }

    private async Task StoreCursor(string cursor)
    {
        await Task.Delay(500);
        var accountId = await GetAccountId();
        var bridgeClient = new RestClient(new RestClientOptions { BaseUrl = new Uri(ApplicationConstants.BridgeServiceUrl) });
        var storeCursorRequest = new RestRequest($"/storage/{ApplicationConstants.AppName}/{accountId}_cursor", 
            Method.Post);
        storeCursorRequest.AddHeader("Blackbird-Token", ApplicationConstants.BlackbirdToken);
        storeCursorRequest.AddBody(cursor);
        await bridgeClient.ExecuteAsync(storeCursorRequest);
    }

    private async Task<string> GetAccountId()
    {
        var dropboxClient = DropboxClientFactory.CreateDropboxClient(Creds);
        var currentAccount = await dropboxClient.Users.GetCurrentAccountAsync();
        
        if (currentAccount == null) 
            throw new Exception("Could not fetch account details.");

        var accountId = currentAccount.AccountId;
        return accountId;
    }
}