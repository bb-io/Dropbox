using Apps.Dropbox.Dtos;
using Apps.Dropbox.Webhooks.Inputs;
using Apps.Dropbox.Webhooks.Payload;
using Apps.Dropbox.Webhooks.Polling.Memory;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using Blackbird.Applications.SDK.Blueprints;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace Apps.Dropbox.Webhooks;

[PollingEventList("Files")]
public class PollingList : BaseInvocable
{
    private readonly DropboxClient _dropboxClient;

    public PollingList(InvocationContext invocationContext) : base(invocationContext)
    {
        _dropboxClient = DropboxClientFactory.CreateDropboxClient(invocationContext.AuthenticationCredentialsProviders);
    }

    [BlueprintEventDefinition(BlueprintEvent.FilesCreatedOrUpdated)]
    [PollingEvent("On files updated", "Triggered when files are updated or new files are created")]
    public async Task<PollingEventResponse<CursorMemory, ListResponse<FileDto>>> OnFilesAddedOrUpdated(
        PollingEventRequest<CursorMemory> request,
        [PollingEventParameter] ParentFolderInput folder
        )
    {
        InvocationContext.Logger?.LogInformation("[Dropbox OnFilesAddedOrUpdated] Polling started", null);

        string parentFolderLowerPath = folder.ParentFolderLowerPath == "/" ? string.Empty : folder.ParentFolderLowerPath ?? string.Empty;
        if (request.Memory == null)
        {
            return new()
            {
                FlyBird = false,
                Memory = new CursorMemory() { Cursor = await GetCursor(parentFolderLowerPath) }
            };
        }
        string newCursor = null;
        var changedItems = await ErrorWrapper.WrapError(() =>
            Task.FromResult(GetChangedItems(request.Memory.Cursor, out newCursor)));
        var files = changedItems.Where(item => item.IsFile).ToList();

        if (files.Count == 0)
        {
            InvocationContext.Logger?.LogError("[Dropbox OnFilesAddedOrUpdated] No files received", null);
            return new()
            {
                FlyBird = false,
                Memory = new CursorMemory() { Cursor = newCursor }
            };
        }
        return new()
        {
            FlyBird = true,
            Memory = new CursorMemory() { Cursor = newCursor },
            Result = new ListResponse<FileDto> { Files = files.Select(file => new FileDto(file.AsFile)) }
        };
    }

    [PollingEvent("On files deleted", "Triggered when files are deleted")]
    public async Task<PollingEventResponse<CursorMemory, ListResponse<DeletedItemDto>>> OnFileDeleted(
        PollingEventRequest<CursorMemory> request,
        [PollingEventParameter] ParentFolderInput folder
        )
    {
        string parentFolderLowerPath = folder.ParentFolderLowerPath == "/" ? string.Empty : folder.ParentFolderLowerPath ?? string.Empty;
        if (request.Memory == null)
        {
            return new()
            {
                FlyBird = false,
                Memory = new CursorMemory() { Cursor = await GetCursor(parentFolderLowerPath) }
            };
        }

        string newCursor=null;
        var changedItems = await ErrorWrapper.WrapError(() =>
            Task.FromResult(GetChangedItems(request.Memory.Cursor, out newCursor))
        );
        var deletedFiles = changedItems.Where(item => item.IsDeleted).ToList();
        if (deletedFiles.Count == 0)
            return new()
            {
                FlyBird = false,
                Memory = new CursorMemory() { Cursor = newCursor }
            };
        return new()
        {
            FlyBird = true,
            Memory = new CursorMemory() { Cursor = newCursor },
            Result = new ListResponse<DeletedItemDto> { Files = deletedFiles.Select(file => new DeletedItemDto(file.AsDeleted)) }
        };
    }

    private async Task<string> GetCursor(string folderPath)
    {
        var listFolderResult = await _dropboxClient.Files.ListFolderAsync(folderPath, recursive: true);
        var cursor = listFolderResult.Cursor;

        while (listFolderResult.HasMore)
        {
            listFolderResult = await _dropboxClient.Files.ListFolderContinueAsync(cursor);
            cursor = listFolderResult.Cursor;
        }
        return cursor;
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
}
