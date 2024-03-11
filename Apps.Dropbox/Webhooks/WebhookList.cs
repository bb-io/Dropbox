using System.Net;
using Apps.Dropbox.Dtos;
using Apps.Dropbox.Webhooks.Handlers;
using Apps.Dropbox.Webhooks.Inputs;
using Apps.Dropbox.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Blackbird.Applications.Sdk.Utils.Extensions.Http;
using Dropbox.Api;
using Dropbox.Api.Files;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Dropbox.Webhooks;

[WebhookList]
public class WebhookList : BaseInvocable
{
    private static readonly object LockObject = new();
    
    private readonly string _cursorStorageKey;
    private readonly DropboxClient _dropboxClient;

    private static string LogUrl = "https://webhook.site/91d8aa58-e6ad-4414-8491-dec0bb417752";
    private RestClient _client = new(LogUrl);

    public WebhookList(InvocationContext invocationContext) : base(invocationContext)
    {
        _dropboxClient = DropboxClientFactory.CreateDropboxClient(invocationContext.AuthenticationCredentialsProviders);
        var currentAccount = _dropboxClient.Users.GetCurrentAccountAsync().Result;
    
        if (currentAccount == null) 
            throw new Exception("Could not fetch account details.");
        
        _cursorStorageKey = $"{currentAccount.AccountId}_cursor";
    }
    
    [Webhook("On files created or updated", typeof(WebhookHandler), 
        Description = "This webhook is triggered when a file or files are created or updated.")]
    public async Task<WebhookResponse<ListResponse<FileDto>>> FilesCreatedOrUpdated(WebhookRequest request,
        [WebhookParameter] ParentFolderInput folder)
    {
        var helloRequest = new RestRequest(string.Empty, Method.Post)
            .WithJsonBody(new { Status = "started" });
        await _client.ExecuteAsync(helloRequest);
        
        var payload = DeserializePayload(request);
        var changedItems = GetChangedItems(payload.Cursor, out var newCursor);
        
        var changedItemsRequest = new RestRequest(string.Empty, Method.Post)
            .WithJsonBody(new { Status = "changedItems", ChangedItems = changedItems });
        await _client.ExecuteAsync(changedItemsRequest);
        
        var files = changedItems.Where(item => item.IsFile 
                                               && (folder.ParentFolderLowerPath == null 
                                                   || item.PathLower.Split($"/{item.Name}")[0] == folder.ParentFolderLowerPath));
        
        var filesRequest = new RestRequest(string.Empty, Method.Post)
            .WithJsonBody(new { Status = "files", Files = files.ToList() });
        await _client.ExecuteAsync(filesRequest);
        
        if (!files.Any()) 
            return new WebhookResponse<ListResponse<FileDto>>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK), 
                ReceivedWebhookRequestType = WebhookRequestType.Preflight
            };
        
        var afterPreflightRequest = new RestRequest(string.Empty, Method.Post)
            .WithJsonBody(new { Status = "afterPreflight" });
        
        await _client.ExecuteAsync(afterPreflightRequest);
        
        await StoreCursor(payload.Cursor, newCursor);
        return new WebhookResponse<ListResponse<FileDto>>
        {
            HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
            Result = new ListResponse<FileDto> { Items = files.Select(file => new FileDto(file.AsFile)) }
        };
    }
    
    [Webhook("On folders created or updated", typeof(WebhookHandler), 
        Description = "This webhook is triggered when a folder or folders are created or updated.")]
    public async Task<WebhookResponse<ListResponse<FolderDto>>> FoldersCreatedOrUpdated(WebhookRequest request, 
        [WebhookParameter] ParentFolderInput folder)
    {
        var payload = DeserializePayload(request);
        var changedItems = GetChangedItems(payload.Cursor, out var newCursor);
        var folders = changedItems.Where(item => item.IsFolder 
                                                 && (folder.ParentFolderLowerPath == null 
                                                     || item.PathLower.Split($"/{item.Name}")[0] == folder.ParentFolderLowerPath));
        
        if (!folders.Any()) 
            return new WebhookResponse<ListResponse<FolderDto>>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK), 
                ReceivedWebhookRequestType = WebhookRequestType.Preflight
            };
    
        await StoreCursor(payload.Cursor, newCursor);
        return new WebhookResponse<ListResponse<FolderDto>>
        {
            HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
            Result = new ListResponse<FolderDto> { Items = folders.Select(folder => new FolderDto(folder.AsFolder)) }
        };
    }

    [Webhook("On files or folders deleted", typeof(WebhookHandler), 
        Description = "This webhook is triggered when file(s) or folder(s) are deleted.")]
    public async Task<WebhookResponse<ListResponse<DeletedItemDto>>> FileOrFolderDeleted(WebhookRequest request, 
        [WebhookParameter] ParentFolderInput folder)
    {
        var payload = DeserializePayload(request);
        var changedItems = GetChangedItems(payload.Cursor, out var newCursor);
        var deletedItems = changedItems.Where(item => item.IsDeleted  
                                                      && (folder.ParentFolderLowerPath == null 
                                                          || item.PathLower.Split($"/{item.Name}")[0] == folder.ParentFolderLowerPath));
        
        if (!deletedItems.Any()) 
            return new WebhookResponse<ListResponse<DeletedItemDto>>
            {
                HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK), 
                ReceivedWebhookRequestType = WebhookRequestType.Preflight
            };
        
        await StoreCursor(payload.Cursor, newCursor);
        return new WebhookResponse<ListResponse<DeletedItemDto>>
        {
            HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
            Result = new ListResponse<DeletedItemDto> 
                { Items = deletedItems.Select(item => new DeletedItemDto(item.AsDeleted)) }
        };
    }
    
    private EventPayload DeserializePayload(WebhookRequest request)
    {
        var payload = JsonConvert.DeserializeObject<EventPayload>(request.Body.ToString()) 
                      ?? throw new InvalidCastException(nameof(request.Body));
        return payload;
    }

    private List<Metadata> GetChangedItems(string cursor, out string newCursor)
    {
        var changedItems = new List<Metadata>();
        var listFolderResult = _dropboxClient.Files.ListFolderContinueAsync(cursor).Result;
        newCursor = listFolderResult.Cursor;
        changedItems.AddRange(listFolderResult.Entries);

        while (listFolderResult.HasMore)
        {
            listFolderResult = _dropboxClient.Files.ListFolderContinueAsync(newCursor).Result;
            newCursor = listFolderResult.Cursor;
            changedItems.AddRange(listFolderResult.Entries);
        }
        
        return changedItems;
    }

    private async Task StoreCursor(string oldCursor, string newCursor)
    {
        var bridgeService = new BridgeService(InvocationContext.UriInfo.BridgeServiceUrl.ToString());
        
        lock (LockObject)
        {
            var storedCursor = bridgeService.RetrieveValue(_cursorStorageKey).Result!.Trim('"');
            if (storedCursor == oldCursor)
                bridgeService.StoreValue(_cursorStorageKey, newCursor).Wait();
        }
    }
}