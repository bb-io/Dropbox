using System.IO.Compression;
using System.Text.RegularExpressions;
using Apps.Dropbox.Dtos;
using Apps.Dropbox.Invocables;
using Apps.Dropbox.Models.Requests;
using Apps.Dropbox.Models.Responses;
using Apps.Dropbox.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Dropbox.Api.Files;
using RestSharp;

namespace Apps.Dropbox.Actions
{
    [ActionList("Files")]
    public class StorageActions(InvocationContext context, IFileManagementClient _fileManagementClient) : DropboxInvocable(context)
    {
        [Action("Search files", Description = "Get files list by specified path")]
        public async Task<FilesResponse> GetFilesListByPath([ActionParameter] FilesRequest input)
        {
            string path = input.Path == "/" ? String.Empty : input.Path;
            var list = await ErrorWrapper.WrapError(() => Client.Files.ListFolderAsync(path));
            var files = list.Entries.Where(e => e.IsFile).Select(f => new FileDto(f.AsFile));
            return new FilesResponse { Files = files };
        }

        [BlueprintActionDefinition(BlueprintAction.UploadFile)]
        [Action("Upload file", Description = "Upload file")]
        public async Task<FileDto> UploadFile( [ActionParameter] UploadFileRequest input)
        {
            var file = await _fileManagementClient.DownloadAsync(input.File);

            var fileBytes = await file.GetByteData();
            var parentFolderPath = string.IsNullOrEmpty(input.ParentFolderPath) ? "/" : input.ParentFolderPath;
            using (var stream = new MemoryStream(fileBytes))
            {
                var response = await ErrorWrapper.WrapError(
                    () => Client.Files.UploadAsync(
                        $"{parentFolderPath.TrimEnd('/')}/{input.File.Name}",
                        WriteMode.Overwrite.Instance, body: stream));

                return new FileDto(response);
            }
        }

        [Action("Delete file", Description = "Delete specified file")]
        public async Task<DeleteResponse> DeleteFile([ActionParameter] DeleteFileRequest input)
        {
            if (string.IsNullOrEmpty(input.FilePath))
            { 
                throw new PluginMisconfigurationException("File path cannot be null or empty. Please check your input and try again");
            }

            var deleteArg = new DeleteArg(input.FilePath);
            var result = await ErrorWrapper.WrapError(() => Client.Files.DeleteV2Async(deleteArg));
            return new DeleteResponse { DeletedObjectPath = result.Metadata.PathDisplay };
        }

        [Action("Move file", Description = "Move file from one folder to another")]
        public async Task<MoveFileResponse> MoveFile([ActionParameter] MoveFileRequest input)
        {
            var filename = FileNameHelper.EnsureCorrectFilename(input.CurrentFilePath, input.TargetFilename);
            var moveArg = new RelocationArg(input.CurrentFilePath, $"{input.DestinationFolder.TrimEnd('/')}/{filename}");
            var result = await ErrorWrapper.WrapError(() => Client.Files.MoveV2Async(moveArg));
            return new MoveFileResponse { FileName = result.Metadata.Name, NewFilePath = result.Metadata.PathDisplay };
        }

        [Action("Copy file", Description = "Copy file from one folder to another")]
        public async Task<MoveFileResponse> CopyFile([ActionParameter] MoveFileRequest input)
        {
            var filename = FileNameHelper.EnsureCorrectFilename(input.CurrentFilePath, input.TargetFilename);
            var copyArg = new RelocationArg(input.CurrentFilePath, $"{input.DestinationFolder.TrimEnd('/')}/{filename}");
            var result = await ErrorWrapper.WrapError(() => Client.Files.CopyV2Async(copyArg));
            return new MoveFileResponse { FileName = result.Metadata.Name, NewFilePath = result.Metadata.PathDisplay };
        }

        [BlueprintActionDefinition(BlueprintAction.DownloadFile)]
        [Action("Download file", Description = "Download specified file")]
        public async Task<DownloadFileResponse> DownloadFile([ActionParameter] DownloadFileRequest input)
        {
            if (!Regex.IsMatch(input.FileId, "\\A(?:(/(.|[\\r\\n])*|id:.*)|(rev:[0-9a-f]{9,})|(ns:[0-9]+(/.*)?))\\z"))
                throw new PluginMisconfigurationException("File path input doesn't match the expected format and seems to be invalid");

            var result = await ErrorWrapper.WrapError(() => Client.Files.GetTemporaryLinkAsync(new GetTemporaryLinkArg(input.FileId)));
            var file = new FileReference(new HttpRequestMessage(HttpMethod.Get, result.Link), result.Metadata.Name, MimeTypes.GetMimeType(result.Metadata.Name));
            return new DownloadFileResponse { File = file };
        }

        [Action("Download all files in folder", Description = "Recursively downloads all files in a folder as a single ZIP")]
        public async Task<DownloadFilesResponse> DownloadAllFiles([ActionParameter] DownloadFolderRequest input)
        {
            if (!Regex.IsMatch(input.FolderPath, "\\A(?:(/(.|[\\r\\n])*)?|id:.*|(ns:[0-9]+(/.*)?))\\z"))
                throw new PluginMisconfigurationException("Folder path input doesn't match the expected format and seems to be invalid");

            var listArg = new ListFolderArg(
                path: input.FolderPath,
                recursive: true,
                includeMediaInfo: false,
                includeDeleted: false,
                includeHasExplicitSharedMembers: false,
                includeMountedFolders: true,
                limit: null,
                sharedLink: null,
                includePropertyGroups: null,
                includeNonDownloadableFiles: false
            );

            var files = new List<FileMetadata>();
            var page = await ErrorWrapper.WrapError(() => Client.Files.ListFolderAsync(listArg));
            files.AddRange(page.Entries.OfType<FileMetadata>());

            while (page.HasMore)
            {
                page = await ErrorWrapper.WrapError(() => Client.Files.ListFolderContinueAsync(page.Cursor));
                files.AddRange(page.Entries.OfType<FileMetadata>());
            }

            var result = new List<FileReference>();

            foreach (var f in files)
            {
                var tmp = await ErrorWrapper.WrapError(() =>
                    Client.Files.GetTemporaryLinkAsync(new GetTemporaryLinkArg(f.Id))
                );

                var mime = MimeTypes.GetMimeType(f.Name);
                var request = new HttpRequestMessage(HttpMethod.Get, tmp.Link);
                var fileRef = new FileReference(request, f.Name, mime);

                result.Add(fileRef);
            }

            return new DownloadFilesResponse
            {
                Files = result
            };
        }

        [Action("DEBUG: Get auth data", Description = "Can be used only for debugging purposes.")]
        public List<AuthenticationCredentialsProvider> GetAuthenticationCredentialsProviders()
        {
            return InvocationContext.AuthenticationCredentialsProviders.ToList();
        }
    }
}
