using System.Net.Mime;
using System.Text.RegularExpressions;
using Apps.Dropbox.Dtos;
using Apps.Dropbox.Models.Requests;
using Apps.Dropbox.Models.Responses;
using Apps.Dropbox.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Dropbox.Api.FileRequests;
using Dropbox.Api.Files;
using Dropbox.Api.Sharing;

namespace Apps.Dropbox.Actions
{
    [ActionList]
    public class StorageActions
    {
        private readonly IFileManagementClient _fileManagementClient;
        public StorageActions(IFileManagementClient fileManagementClient)
        {
            _fileManagementClient = fileManagementClient;
        }

        [Action("Get folders list by path", Description = "Get folders list by specified path")]
        public async Task<FoldersResponse> GetFoldersListByPath(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] FoldersRequest input)
        {
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);

            string path = input.Path == "/" ? String.Empty : input.Path;
            var list = await ErrorWrapper.WrapError(() => dropboxClient.Files.ListFolderAsync(path));
            var folders = list.Entries.Where(e => e.IsFolder).Select(f => new FolderDto(f.AsFolder));
            return new FoldersResponse { Folders = folders };
        }

        [Action("Get files list by path", Description = "Get files list by specified path")]
        public async Task<FilesResponse> GetFilesListByPath(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] FilesRequest input)
        {
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);

            string path = input.Path == "/" ? String.Empty : input.Path;
            var list = await ErrorWrapper.WrapError(() => dropboxClient.Files.ListFolderAsync(path));
            var files = list.Entries.Where(e => e.IsFile).Select(f => new FileDto(f.AsFile));
            return new FilesResponse { Files = files };
        }

        [Action("Create folder", Description = "Create folder with a given name")]
        public async Task<FolderDto> CreateFolder(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] CreateFolderRequest input)
        {
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
            var folderArg = new CreateFolderArg($"{input.ParentFolderPath.TrimEnd('/')}/{input.FolderName}");
            var result = await ErrorWrapper.WrapError(() => dropboxClient.Files.CreateFolderV2Async(folderArg));
            return new FolderDto(result.Metadata.AsFolder);
        }

        [BlueprintActionDefinition(BlueprintAction.UploadFile)]
        [Action("Upload file", Description = "Upload file")]
        public async Task<FileDto> UploadFile(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] UploadFileRequest input)
        {
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
            var file = await _fileManagementClient.DownloadAsync(input.File);
            var fileBytes = await file.GetByteData();
            var parentFolderPath = string.IsNullOrEmpty(input.ParentFolderPath) ? "/" : input.ParentFolderPath;
            using (var stream = new MemoryStream(fileBytes))
            {
                var response = await ErrorWrapper.WrapError(
                    () => dropboxClient.Files.UploadAsync(
                        $"{parentFolderPath.TrimEnd('/')}/{input.File.Name}",
                        WriteMode.Overwrite.Instance, body: stream));

                return new FileDto(response);
            }
        }

        [Action("Delete file", Description = "Delete specified file")]
        public async Task<DeleteResponse> DeleteFile(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] DeleteFileRequest input)
        {
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
            var deleteArg = new DeleteArg(input.FilePath);
            var result = await ErrorWrapper.WrapError(() => dropboxClient.Files.DeleteV2Async(deleteArg));
            return new DeleteResponse { DeletedObjectPath = result.Metadata.PathDisplay };
        }

        [Action("Delete folder", Description = "Delete specified folder")]
        public async Task<DeleteResponse> DeleteFolder(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] DeleteFolderRequest input)
        {
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
            var deleteArg = new DeleteArg(input.FolderPath);
            var result = await ErrorWrapper.WrapError(async () => await dropboxClient.Files.DeleteV2Async(deleteArg));
            return new DeleteResponse { DeletedObjectPath = result.Metadata.PathDisplay };
        }

        [Action("Move file", Description = "Move file from one directory to another")]
        public async Task<MoveFileResponse> MoveFile(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] MoveFileRequest input)
        {
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
            var filename = FileNameHelper.EnsureCorrectFilename(input.CurrentFilePath, input.TargetFilename);
            var moveArg = new RelocationArg(input.CurrentFilePath, $"{input.DestinationFolder.TrimEnd('/')}/{filename}");
            var result = await ErrorWrapper.WrapError(() => dropboxClient.Files.MoveV2Async(moveArg));
            return new MoveFileResponse { FileName = result.Metadata.Name, NewFilePath = result.Metadata.PathDisplay };
        }

        [Action("Copy file", Description = "Copy file from one directory to another")]
        public async Task<MoveFileResponse> CopyFile(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] MoveFileRequest input)
        {
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
            var filename = FileNameHelper.EnsureCorrectFilename(input.CurrentFilePath, input.TargetFilename);
            var copyArg = new RelocationArg(input.CurrentFilePath, $"{input.DestinationFolder.TrimEnd('/')}/{filename}");
            var result = await ErrorWrapper.WrapError(() => dropboxClient.Files.CopyV2Async(copyArg));
            return new MoveFileResponse { FileName = result.Metadata.Name, NewFilePath = result.Metadata.PathDisplay };
        }

        [Action("Create file request", Description = "Create file request for current user")]
        public async Task<CreateFileRequestResponse> CreateFileRequest(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] CreateFileRequestRequest input)
        {
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
            var createFileArg = new CreateFileRequestArgs(input.RequestTitle, input.Destination);
            var result = await ErrorWrapper.WrapError(() => dropboxClient.FileRequests.CreateAsync(createFileArg));
            return new CreateFileRequestResponse { RequestUrl = result.Url, Destination = result.Destination };
        }

        [Action("Share folder", Description = "Share given folder")]
        public async Task<ShareFolderResponse> ShareFolder(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] ShareFolderRequest input)
        {
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
            var shareFolderArg = new ShareFolderArg(input.FolderPath);
            var result = await ErrorWrapper.WrapError(() => dropboxClient.Sharing.ShareFolderAsync(shareFolderArg));
            return new ShareFolderResponse
            {
                IsComplete = result.IsComplete,
                IsAsyncJob = result.IsAsyncJobId,
                Name = result.AsComplete.Value.Name,
                PreviewUrl = result.AsComplete.Value.PreviewUrl,
                SharedFolderId = result.AsComplete.Value.SharedFolderId
            };
        }

        [BlueprintActionDefinition(BlueprintAction.DownloadFile)]
        [Action("Download file", Description = "Download specified file")]
        public async Task<DownloadFileResponse> DownloadFile(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] DownloadFileRequest input)
        {
            if (!Regex.IsMatch(input.FileId, "\\A(?:(/(.|[\\r\\n])*|id:.*)|(rev:[0-9a-f]{9,})|(ns:[0-9]+(/.*)?))\\z"))
                throw new PluginMisconfigurationException("File path input doesn't match the expected format and seems to be invalid");
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
            var downloadArg = new DownloadArg(input.FileId);
            using var response = await ErrorWrapper.WrapError(() => dropboxClient.Files.DownloadAsync(downloadArg));
            var filename = response.Response.AsFile.Name;
            var fileStream = await response.GetContentAsStreamAsync();
            var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            var file = await _fileManagementClient.UploadAsync(memoryStream, MediaTypeNames.Application.Octet, filename);
            return new DownloadFileResponse { File = file };
        }

        [Action("Get link for file download", Description = "Get temporary link for download of a file")]
        public async Task<GetDownloadLinkResponse> GetDownloadLink(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] DownloadFileRequest input)
        {
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
            var getLinkArg = new GetTemporaryLinkArg(input.FileId);
            var result = await ErrorWrapper.WrapError(() => dropboxClient.Files.GetTemporaryLinkAsync(getLinkArg));
            return new GetDownloadLinkResponse{LinkForDownload = result.Link,Path = result.Metadata.PathDisplay,SizeInBytes = result.Metadata.Size};
        }
    }
}
