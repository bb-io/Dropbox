using System.Net.Mime;
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
using Dropbox.Api.FileRequests;
using Dropbox.Api.Files;

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
            //var downloadArg = new DownloadArg(input.FileId);
            //using var response = await ErrorWrapper.WrapError(() => Client.Files.DownloadAsync(downloadArg));
            //var filename = response.Response.AsFile.Name;
            //var fileStream = await response.GetContentAsStreamAsync();
            //var memoryStream = new MemoryStream();
            //await fileStream.CopyToAsync(memoryStream);
            //memoryStream.Position = 0;

            var result = await ErrorWrapper.WrapError(() => Client.Files.GetTemporaryLinkAsync(new GetTemporaryLinkArg(input.FileId)));
            var file = new FileReference(new HttpRequestMessage(HttpMethod.Get, result.Link), result.Metadata.Name, MimeTypes.GetMimeType(result.Metadata.Name));
            //var file = await _fileManagementClient.UploadAsync(memoryStream, MediaTypeNames.Application.Octet, filename);
            return new DownloadFileResponse { File = file };
        }

        //[Action("Get link for file download", Description = "Get temporary link for download of a file")]
        //public async Task<GetDownloadLinkResponse> GetDownloadLink([ActionParameter] DownloadFileRequest input)
        //{
        //    var getLinkArg = new GetTemporaryLinkArg(input.FileId);
        //    var result = await ErrorWrapper.WrapError(() => Client.Files.GetTemporaryLinkAsync(getLinkArg));
        //    return new GetDownloadLinkResponse{LinkForDownload = result.Link,Path = result.Metadata.PathDisplay,SizeInBytes = result.Metadata.Size};
        //}
    }
}
