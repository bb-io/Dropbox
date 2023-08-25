using Apps.Dropbox.Dtos;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Dropbox.Models.Responses;
using Apps.Dropbox.Models.Requests;
using Dropbox.Api.Files;
using Dropbox.Api.FileRequests;
using Dropbox.Api.Sharing;
using Blackbird.Applications.Sdk.Common.Actions;
using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.Dropbox.Actions
{
    [ActionList]
    public class StorageActions
    {
        [Action("Get folders list by path", Description = "Get folders list by specified path")]
        public async Task<FoldersResponse> GetFoldersListByPath(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] FoldersRequest input)
        {
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
            var list = await dropboxClient.Files.ListFolderAsync(input.Path);
            var folders = list.Entries.Where(e => e.IsFolder).Select(f => new FolderDto(f.AsFolder));
            return new FoldersResponse { Folders = folders };
        }

        [Action("Get files list by path", Description = "Get files list by specified path")]
        public async Task<FilesResponse> GetFilesListByPath(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] FilesRequest input)
        {
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
            var list = await dropboxClient.Files.ListFolderAsync(input.Path);
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
            
            using (var stream = new MemoryStream(input.File.Bytes))
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
            var result = await dropboxClient.Files.DeleteV2Async(deleteArg);
            return new DeleteResponse { DeletedObjectPath = result.Metadata.PathDisplay };
        }

        [Action("Move file", Description = "Move file from one directory to another")]
        public async Task<MoveFileResponse> MoveFile(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] MoveFileRequest input)
        {
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
            var filename = input.TargetFilename ?? input.CurrentFilePath.Split("/")[^1];
            var moveArg = new RelocationArg(input.CurrentFilePath, $"{input.DestinationFolder.TrimEnd('/')}/{filename}");
            var result = await dropboxClient.Files.MoveV2Async(moveArg);
            
            return new MoveFileResponse
            {
                FileName = result.Metadata.Name,
                NewFilePath = result.Metadata.PathDisplay
            };
        }

        [Action("Copy file", Description = "Copy file from one directory to another")]
        public async Task<MoveFileResponse> CopyFile(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] MoveFileRequest input)
        {
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
            var filename = input.TargetFilename ?? input.CurrentFilePath.Split("/")[^1];
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
        public void ShareFolder(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] ShareFolderRequest input)
        {
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
            var shareFolderArg = new ShareFolderArg(input.FolderPath);
            dropboxClient.Sharing.ShareFolderAsync(shareFolderArg);
        }

        [Action("Download file", Description = "Download specified file")]
        public async Task<DownloadFileResponse> DownloadFile(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] DownloadFileRequest input)
        {
            var dropboxClient = DropboxClientFactory.CreateDropboxClient(authenticationCredentialsProviders);
            var downloadArg = new DownloadArg(input.FilePath);
            
            using (var response = await dropboxClient.Files.DownloadAsync(downloadArg))
            {
                var filename = response.Response.AsFile.Name;
                
                if (!MimeTypes.TryGetMimeType(filename, out var contentType))
                    contentType = "application/octet-stream";
                
                byte[] file = await response.GetContentAsByteArrayAsync();
                return new DownloadFileResponse 
                { 
                    File = new File(file)
                    {
                        Name = filename,
                        ContentType = contentType
                    } 
                };
            }
        }

        [Action("Get link for file download", Description = "Get temporary link for download of a file")]
        public async Task<GetDownloadLinkResponse> GetDownloadLink(
            IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] DownloadFileRequest input)
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
    }
}
