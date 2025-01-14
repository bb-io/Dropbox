using System.Net.Mime;
using System.Text.RegularExpressions;
using Apps.Dropbox.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Dropbox.Models.Responses;
using Apps.Dropbox.Models.Requests;
using Apps.Dropbox.Utils;
using Dropbox.Api.Files;
using Dropbox.Api;
using Dropbox.Api.FileRequests;
using Dropbox.Api.Sharing;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Dropbox.Actions
{
    [ActionList]
    public class StorageActions
    {
        private readonly IFileManagementClient _fileManagementClient;
        private readonly Dictionary<string, string> _dropboxMoveFileErrorMessages = new()
        {
            { "cant_copy_shared_folder", "Shared folders can't be copied." },
            { "cant_nest_shared_folder", "Your move operation would result in nested shared folders. This is not allowed." },
            { "cant_move_folder_into_itself", "You cannot move a folder into itself." },
            { "too_many_files", "The operation would involve more than 10,000 files and folders." },
            { "duplicated_or_nested_paths", "There are duplicated/nested paths among Current or Destination path." },
            { "cant_transfer_ownership", "Your move operation would result in an ownership transfer. Check the ownership permision." },
            { "insufficient_quota", "The current user does not have enough space to move or copy the files." },
            { "internal_error", "Something went wrong on Dropbox's end. Please verify the action succeeded, and if not, try again." }
        };
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
            var list = await dropboxClient.Files.ListFolderAsync(path);
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
            var list = await dropboxClient.Files.ListFolderAsync(path);
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
            var result = await dropboxClient.Files.CreateFolderV2Async(folderArg);
            return new FolderDto(result.Metadata.AsFolder);
        }

        [Action("Upload file", Description = "Upload file")]
        public async Task<FileDto> UploadFile(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] UploadFileRequest input)
        {
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
            var file = await _fileManagementClient.DownloadAsync(input.File);
            var fileBytes = await file.GetByteData();

            using (var stream = new MemoryStream(fileBytes))
            {
                var response = await dropboxClient.Files.UploadAsync(
                    $"{input.ParentFolderPath.TrimEnd('/')}/{input.File.Name}",
                    WriteMode.Overwrite.Instance, body: stream);
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
            var result = await dropboxClient.Files.DeleteV2Async(deleteArg);
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
            try
            {
                var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
                var filename = FileNameHelper.EnsureCorrectFilename(input.CurrentFilePath, input.TargetFilename);
                var moveArg = new RelocationArg(input.CurrentFilePath, $"{input.DestinationFolder.TrimEnd('/')}/{filename}");
                var result = await dropboxClient.Files.MoveV2Async(moveArg);

                return new MoveFileResponse
                {
                    FileName = result.Metadata.Name,
                    NewFilePath = result.Metadata.PathDisplay
                };
            }
            catch (global::Dropbox.Api.ApiException<RelocationError> ex)
            {
                var messageParts = ex.Message?.Split('/') ?? Array.Empty<string>();
                var errorTag = messageParts
                                .FirstOrDefault(part => _dropboxMoveFileErrorMessages.ContainsKey(part))
                                ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(errorTag))
                {
                    var userFriendlyMessage = _dropboxMoveFileErrorMessages[errorTag];
                    throw new PluginMisconfigurationException(
                        $"We couldn't move your file: {userFriendlyMessage}."
                    );
                }
                throw new PluginMisconfigurationException($"We couldn't move your file: {ex.Message}. Please check the file paths and try again.");
            }
            catch (Exception ex)
            {
                throw new PluginApplicationException($"An unexpected error occurred while moving the file: {ex.Message}.");
            }
        }

        [Action("Copy file", Description = "Copy file from one directory to another")]
        public async Task<MoveFileResponse> CopyFile(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] MoveFileRequest input)
        {
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
            var filename = FileNameHelper.EnsureCorrectFilename(input.CurrentFilePath, input.TargetFilename);
            var copyArg = new RelocationArg(input.CurrentFilePath, $"{input.DestinationFolder.TrimEnd('/')}/{filename}");
            var result = await dropboxClient.Files.CopyV2Async(copyArg);

            return new MoveFileResponse
            {
                FileName = result.Metadata.Name,
                NewFilePath = result.Metadata.PathDisplay
            };
        }

        [Action("Create file request", Description = "Create file request for current user")]
        public async Task<CreateFileRequestResponse> CreateFileRequest(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] CreateFileRequestRequest input)
        {
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
            var createFileArg = new CreateFileRequestArgs(input.RequestTitle, input.Destination);
            var result = await dropboxClient.FileRequests.CreateAsync(createFileArg);

            return new CreateFileRequestResponse
            {
                RequestUrl = result.Url,
                Destination = result.Destination
            };
        }

        [Action("Share folder", Description = "Share given folder")]
        public async Task<ShareFolderResponse> ShareFolder(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] ShareFolderRequest input)
        {
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
            var shareFolderArg = new ShareFolderArg(input.FolderPath);
            var result = await dropboxClient.Sharing.ShareFolderAsync(shareFolderArg);

            return new ShareFolderResponse
            {
                IsComplete = result.IsComplete,
                IsAsyncJob = result.IsAsyncJobId,
                Name = result.AsComplete.Value.Name,
                PreviewUrl = result.AsComplete.Value.PreviewUrl,
                SharedFolderId = result.AsComplete.Value.SharedFolderId
            };
        }

        [Action("Download file", Description = "Download specified file")]
        public async Task<DownloadFileResponse> DownloadFile(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] DownloadFileRequest input)
        {
            if (!Regex.IsMatch(input.FilePath, "\\A(?:(/(.|[\\r\\n])*|id:.*)|(rev:[0-9a-f]{9,})|(ns:[0-9]+(/.*)?))\\z"))
                throw new("File path input doesn't match the expected format and seems to be invalid");

            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
            var downloadArg = new DownloadArg(input.FilePath);

            using var response = await dropboxClient.Files.DownloadAsync(downloadArg);
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
            try
            {
                var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
                var getLinkArg = new GetTemporaryLinkArg(input.FilePath);
                var result = await dropboxClient.Files.GetTemporaryLinkAsync(getLinkArg);

                return new GetDownloadLinkResponse
                {
                    LinkForDownload = result.Link,
                    Path = result.Metadata.PathDisplay,
                    SizeInBytes = result.Metadata.Size
                };
            }
            catch (global::Dropbox.Api.ApiException<GetTemporaryLinkError> ex)
            {
                throw new PluginMisconfigurationException($"Something went wrong while communicating with Dropbox: {ex.Message}. Please double-check your file path and try again.");
            }
            catch (Exception ex)
            {
                throw new PluginApplicationException($"An unexpected error occurred while moving the file: {ex.Message}.");
            }
        }
    }
} 
