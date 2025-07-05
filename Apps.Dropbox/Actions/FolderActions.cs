using Apps.Dropbox.Dtos;
using Apps.Dropbox.Invocables;
using Apps.Dropbox.Models.Requests;
using Apps.Dropbox.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Dropbox.Api.Files;
using Dropbox.Api.Sharing;

namespace Apps.Dropbox.Actions
{
    [ActionList("Folders")]
    public class FolderActions(InvocationContext context) : DropboxInvocable(context)
    {
        [Action("Search folders", Description = "Get folders list by specified path")]
        public async Task<FoldersResponse> GetFoldersListByPath([ActionParameter] FoldersRequest input)
        {
            string path = input.Path == "/" ? String.Empty : input.Path;
            var list = await ErrorWrapper.WrapError(() => Client.Files.ListFolderAsync(path));
            var folders = list.Entries.Where(e => e.IsFolder).Select(f => new FolderDto(f.AsFolder));
            return new FoldersResponse { Folders = folders };
        }

        [Action("Create folder", Description = "Create folder with a given name")]
        public async Task<FolderDto> CreateFolder([ActionParameter] CreateFolderRequest input)
        {
            var folderArg = new CreateFolderArg($"{input.ParentFolderPath.TrimEnd('/')}/{input.FolderName}");
            var result = await ErrorWrapper.WrapError(() => Client.Files.CreateFolderV2Async(folderArg));
            return new FolderDto(result.Metadata.AsFolder);
        }

        [Action("Delete folder", Description = "Delete specified folder")]
        public async Task<DeleteResponse> DeleteFolder([ActionParameter] DeleteFolderRequest input)
        {
            var deleteArg = new DeleteArg(input.FolderPath);
            var result = await ErrorWrapper.WrapError(async () => await Client.Files.DeleteV2Async(deleteArg));
            return new DeleteResponse { DeletedObjectPath = result.Metadata.PathDisplay };
        }

        [Action("Share folder", Description = "Share given folder")]
        public async Task<ShareFolderResponse> ShareFolder([ActionParameter] ShareFolderRequest input)
        {
            var shareFolderArg = new ShareFolderArg(input.FolderPath);
            var result = await ErrorWrapper.WrapError(() => Client.Sharing.ShareFolderAsync(shareFolderArg));
            return new ShareFolderResponse
            {
                IsComplete = result.IsComplete,
                IsAsyncJob = result.IsAsyncJobId,
                Name = result.AsComplete.Value.Name,
                PreviewUrl = result.AsComplete.Value.PreviewUrl,
                SharedFolderId = result.AsComplete.Value.SharedFolderId
            };
        }
    }
}
