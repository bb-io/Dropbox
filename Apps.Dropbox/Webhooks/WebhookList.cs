using System.Net;
using Apps.Dropbox.Dtos;
using Apps.Dropbox.Webhooks.Handlers;
using Apps.Dropbox.Webhooks.Inputs;
using Apps.Dropbox.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Dropbox.Api;
using Dropbox.Api.Files;
using Newtonsoft.Json;

namespace Apps.Dropbox.Webhooks;

[WebhookList]
public class WebhookList : BaseInvocable
{
    private readonly string _cursorStorageKey;
    private readonly DropboxClient _dropboxClient;

    public WebhookList(InvocationContext invocationContext) : base(invocationContext)
    {
        _dropboxClient = DropboxClientFactory.CreateDropboxClient(invocationContext.AuthenticationCredentialsProviders);
        var currentAccount = _dropboxClient.Users.GetCurrentAccountAsync().Result;

        if (currentAccount == null)
            throw new Exception("Could not fetch account details.");

        _cursorStorageKey = $"{currentAccount.AccountId}_cursor";
    }

    //[Webhook("On files created or updated", typeof(WebhookHandler),
    //    Description = "This webhook is triggered when a file or files are created or updated.")]
    //public async Task<WebhookResponse<ListResponse<FileDto>>> FilesCreatedOrUpdated(WebhookRequest request,
    //    [WebhookParameter] ParentFolderInput folder)
    //{
    //    var payload = DeserializePayload(request);
    //    var changedItems = GetChangedItems(payload.Cursor, out var newCursor);

    //    await StoreCursor(payload.Cursor, newCursor);
        
    //    string parentFolderLowerPath = folder.ParentFolderLowerPath == "/" 
    //        ? string.Empty 
    //        : folder.ParentFolderLowerPath ?? string.Empty;
    //    var files = changedItems.Where(item => item.IsFile &&
    //                                           (folder.ParentFolderLowerPath == null ||
    //                                            item.PathLower.StartsWith(parentFolderLowerPath + "/"))).ToList();
    //    if (files.Count == 0)
    //    {
    //        return new WebhookResponse<ListResponse<FileDto>>
    //        {
    //            HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
    //            ReceivedWebhookRequestType = WebhookRequestType.Preflight
    //        };
    //    }

    //    return new WebhookResponse<ListResponse<FileDto>>
    //    {
    //        HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
    //        Result = new ListResponse<FileDto> { Items = files.Select(file => new FileDto(file.AsFile)) }
    //    };
    //}

    //[Webhook("On folders created or updated", typeof(WebhookHandler),
    //    Description = "This webhook is triggered when a folder or folders are created or updated.")]
    //public async Task<WebhookResponse<ListResponse<FolderDto>>> FoldersCreatedOrUpdated(WebhookRequest request,
    //    [WebhookParameter] ParentFolderInput folder)
    //{
    //    var payload = DeserializePayload(request);
    //    var changedItems = GetChangedItems(payload.Cursor, out var newCursor);

    //    await StoreCursor(payload.Cursor, newCursor);

    //    string parentFolderLowerPath = folder.ParentFolderLowerPath == "/" 
    //        ? string.Empty 
    //        : folder.ParentFolderLowerPath ?? string.Empty;
    //    var folders = changedItems.Where(item => item.IsFolder
    //                                             && (folder.ParentFolderLowerPath == null
    //                                                 || item.PathLower.StartsWith(parentFolderLowerPath + "/"))).ToList();
    //    if (folders.Count == 0)
    //    {
    //        return new WebhookResponse<ListResponse<FolderDto>>
    //        {
    //            HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
    //            ReceivedWebhookRequestType = WebhookRequestType.Preflight
    //        };
    //    }

    //    return new WebhookResponse<ListResponse<FolderDto>>
    //    {
    //        HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
    //        Result = new ListResponse<FolderDto> { Items = folders.Select(folder => new FolderDto(folder.AsFolder)) }
    //    };
    //}

    //[Webhook("On files or folders deleted", typeof(WebhookHandler),
    //    Description = "This webhook is triggered when file(s) or folder(s) are deleted.")]
    //public async Task<WebhookResponse<ListResponse<DeletedItemDto>>> FileOrFolderDeleted(WebhookRequest request,
    //    [WebhookParameter] ParentFolderInput folder)
    //{
    //    var payload = DeserializePayload(request);
    //    var changedItems = GetChangedItems(payload.Cursor, out var newCursor);

    //    await StoreCursor(payload.Cursor, newCursor);
        
    //    string parentFolderLowerPath = folder.ParentFolderLowerPath == "/" 
    //        ? string.Empty 
    //        : folder.ParentFolderLowerPath ?? string.Empty;
    //    var deletedItems = changedItems.Where(item => item.IsDeleted
    //                                                  && (folder.ParentFolderLowerPath == null
    //                                                      || item.PathLower.StartsWith(parentFolderLowerPath + "/"))).ToList();
    //    if (deletedItems.Count == 0)
    //    {
    //        return new WebhookResponse<ListResponse<DeletedItemDto>>
    //        {
    //            HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
    //            ReceivedWebhookRequestType = WebhookRequestType.Preflight
    //        };
    //    }

    //    return new WebhookResponse<ListResponse<DeletedItemDto>>
    //    {
    //        HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK),
    //        Result = new ListResponse<DeletedItemDto>
    //            { Items = deletedItems.Select(item => new DeletedItemDto(item.AsDeleted)) }
    //    };
    //}

    //private EventPayload DeserializePayload(WebhookRequest request)
    //{
    //    var payload = JsonConvert.DeserializeObject<EventPayload>(request.Body.ToString())
    //                  ?? throw new InvalidCastException(nameof(request.Body));
    //    return payload;
    //}

    //private List<Metadata> GetChangedItems(string cursor, out string newCursor)
    //{
    //    var changedItems = new List<Metadata>();
    //    var listFolderResult = _dropboxClient.Files.ListFolderContinueAsync(cursor).Result;
    //    newCursor = listFolderResult.Cursor;
    //    changedItems.AddRange(listFolderResult.Entries);

    //    while (listFolderResult.HasMore)
    //    {
    //        listFolderResult = _dropboxClient.Files.ListFolderContinueAsync(newCursor).Result;
    //        newCursor = listFolderResult.Cursor;
    //        changedItems.AddRange(listFolderResult.Entries);
    //    }

    //    return changedItems;
    //}

    //private async Task StoreCursor(string oldCursor, string newCursor)
    //{
    //    var bridgeService = new BridgeService(InvocationContext.UriInfo.BridgeServiceUrl.ToString());

    //    var storedCursor = bridgeService.RetrieveValue(_cursorStorageKey).Result?.Trim('"');
    //    if (storedCursor is null || oldCursor == storedCursor)
    //        await bridgeService.StoreValue(_cursorStorageKey, newCursor);
    //}
}