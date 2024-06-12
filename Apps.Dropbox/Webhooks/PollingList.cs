using Apps.Dropbox.Dtos;
using Apps.Dropbox.Webhooks.Inputs;
using Apps.Dropbox.Webhooks.Payload;
using Apps.Dropbox.Webhooks.Polling.Memory;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace Apps.Dropbox.Webhooks
{
    [PollingEventList]
    public class PollingList : BaseInvocable
    {
        private readonly DropboxClient _dropboxClient;

        public PollingList(InvocationContext invocationContext) : base(invocationContext)
        {
            _dropboxClient = DropboxClientFactory.CreateDropboxClient(invocationContext.AuthenticationCredentialsProviders);
        }

        [PollingEvent("On files created or updated", "On files created or updated")]
        public async Task<PollingEventResponse<CursorMemory, ListResponse<FileDto>>> OnFilesAddedOrUpdated(
            PollingEventRequest<CursorMemory> request,
            [PollingEventParameter] ParentFolderInput folder
            )
        {
            if(request.Memory == null)
            {
                return new()
                {
                    FlyBird = false,
                    Memory = new CursorMemory() { Cursor = await GetCursor(folder.ParentFolderLowerPath ?? string.Empty) }
                };
            }
            var changedItems = GetChangedItems(request.Memory.Cursor, out var newCursor);
            //string parentFolderLowerPath = folder.ParentFolderLowerPath == "/" ? string.Empty : folder.ParentFolderLowerPath ?? string.Empty;
            var files = changedItems.Where(item => item.IsFile).ToList(); //&& (folder.ParentFolderLowerPath == null || item.PathLower.StartsWith(parentFolderLowerPath + "/"))
            if(files.Count == 0)
                return new()
                {
                    FlyBird = false,
                    Memory = new CursorMemory() { Cursor = newCursor }
                };
            return new()
            {
                FlyBird = true,
                Memory = new CursorMemory() { Cursor = newCursor },
                Result = new ListResponse<FileDto> { Items = files.Select(file => new FileDto(file.AsFile)) }
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
}
