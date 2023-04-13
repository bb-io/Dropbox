using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Dropbox.Models.Responses;
using Apps.Dropbox.Models.Requests;
using Dropbox.Api;
using Dropbox.Api.Common;
using Dropbox.Api.Files;
using Dropbox.Api.Team;
using System.Net.Http;
using static Dropbox.Api.Files.SearchMatchTypeV2;
using System.Text;
using Dropbox.Api.FileRequests;
using Dropbox.Api.Sharing;

namespace Apps.Dropbox.Actions
{
    [ActionList]
    public class Actions
    {
        [Action("Get folders list by path", Description = "Get folders list by specified path")]
        public FoldersResponse GetFoldersListByPath(string applicationName, AuthenticationCredentialsProvider authenticationCredentialsProvider,
            [ActionParameter] FoldersRequest input)
        {
            var dropBoxClient = CreateDropboxClient(authenticationCredentialsProvider.Value, applicationName);
            
            string foldersNames = "";
            var list = dropBoxClient.Files.ListFolderAsync(input.Path).Result;
            foreach (var item in list.Entries.Where(i => i.IsFolder))
            {
                foldersNames += item.Name + ", ";
            }
            return new FoldersResponse()
            {
                FolderNames = foldersNames
            };
        }

        [Action("Get files list by path", Description = "Get files list by specified path")]
        public FilesResponse GetFilesListByPath(string applicationName, AuthenticationCredentialsProvider authenticationCredentialsProvider,
            [ActionParameter] FilesRequest input)
        {
            var dropBoxClient = CreateDropboxClient(authenticationCredentialsProvider.Value, applicationName);

            string filesNames = "";
            var list = dropBoxClient.Files.ListFolderAsync(input.Path).Result;
            foreach (var item in list.Entries.Where(i => i.IsFile))
            {
                filesNames += item.Name + ", ";
            }
            return new FilesResponse()
            {
                FileNames = filesNames
            };
        }

        [Action("Create folder", Description = "Create folder with a given name")]
        public CreateFolderResponse CreateFolder(string applicationName, AuthenticationCredentialsProvider authenticationCredentialsProvider,
            [ActionParameter] CreateFolderRequest input)
        {
            var dropBoxClient = CreateDropboxClient(authenticationCredentialsProvider.Value, applicationName);

            var folderArg = new CreateFolderArg($"{input.Path.TrimEnd('/')}/{input.FolderName}");
            var result = dropBoxClient.Files.CreateFolderV2Async(folderArg).Result;


            return new CreateFolderResponse()
            {
                FolderPath = result.Metadata.PathDisplay
            };
        }

        [Action("Upload file", Description = "Upload file")]
        public BaseResponse UploadFile(string applicationName, AuthenticationCredentialsProvider authenticationCredentialsProvider,
            [ActionParameter] UploadFileRequest input)
        {
            var dropBoxClient = CreateDropboxClient(authenticationCredentialsProvider.Value, applicationName);

            var response = new FileMetadata();

            // Testing purposes
            byte[] buffer = new byte[] { 0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x20, 0x66, 0x69, 0x6c, 0x65 };

            using (var stream = new MemoryStream(buffer)) // should be input.File when will be able to upload file
            {
                response = dropBoxClient.Files.UploadAsync($"{input.Path.TrimEnd('/')}/{input.Filename}.{input.FileType}", WriteMode.Overwrite.Instance, body: stream).Result;
            }
            
            return new BaseResponse()
            {
                StatusCode = 200,
                Details = $"File uploaded succesfully. File size: {response.Size}, path: {response.PathDisplay}"
            };
        }

        [Action("Delete", Description = "Delete specified folder or file")]
        public DeleteResponse DeleteFolder(string applicationName, AuthenticationCredentialsProvider authenticationCredentialsProvider,
            [ActionParameter] DeleteRequest input)
        {
            var dropBoxClient = CreateDropboxClient(authenticationCredentialsProvider.Value, applicationName);

            var objectArg = new DeleteArg($"{input.Path.TrimEnd('/')}/{input.ObjectToDeleteName}");
            var result = dropBoxClient.Files.DeleteV2Async(objectArg).Result;


            return new DeleteResponse()
            {
                DeletedObjectPath = result.Metadata.PathDisplay
            };
        }

        [Action("Move file", Description = "Move file from one directory to another")]
        public MoveFileResponse MoveFile(string applicationName, AuthenticationCredentialsProvider authenticationCredentialsProvider,
            [ActionParameter] MoveFileRequest input)
        {
            var dropBoxClient = CreateDropboxClient(authenticationCredentialsProvider.Value, applicationName);

            var moveArg = new RelocationArg($"{input.PathFrom.TrimEnd('/')}/{input.SourceFileName}", $"{input.PathTo.TrimEnd('/')}/{input.TargetFileName}");
            var result = dropBoxClient.Files.MoveV2Async(moveArg).Result;

            return new MoveFileResponse()
            {
                FileName = result.Metadata.Name,
                NewFilePath = result.Metadata.PathDisplay
            };
        }

        [Action("Copy file", Description = "Copy file from one directory to another")]
        public MoveFileResponse CopyFile(string applicationName, AuthenticationCredentialsProvider authenticationCredentialsProvider,
            [ActionParameter] MoveFileRequest input)
        {
            var dropBoxClient = CreateDropboxClient(authenticationCredentialsProvider.Value, applicationName);

            var copyArg = new RelocationArg($"{input.PathFrom.TrimEnd('/')}/{input.SourceFileName}", $"{input.PathTo.TrimEnd('/')}/{input.TargetFileName}");
            var result = dropBoxClient.Files.CopyV2Async(copyArg).Result;

            return new MoveFileResponse()
            {
                FileName = result.Metadata.Name,
                NewFilePath = result.Metadata.PathDisplay
            };
        }

        [Action("Create file request", Description = "Create file request for current user")]
        public CreateFileRequestResponse CreateFileRequest(string applicationName, AuthenticationCredentialsProvider authenticationCredentialsProvider,
            [ActionParameter] CreateFileRequestRequest input)
        {
            var dropBoxClient = CreateDropboxClient(authenticationCredentialsProvider.Value, applicationName);

            var createFileArg = new CreateFileRequestArgs(input.RequestTitle,input.Destination);
            var result = dropBoxClient.FileRequests.CreateAsync(createFileArg).Result;


            return new CreateFileRequestResponse()
            {
                RequestUrl = result.Url,
                Destination = result.Destination
            };
        }

        [Action("Share folder", Description = "Share given folder")]
        public void ShareFolder(string applicationName, AuthenticationCredentialsProvider authenticationCredentialsProvider,
            [ActionParameter] ShareFolderRequest input)
        {
            var dropBoxClient = CreateDropboxClient(authenticationCredentialsProvider.Value, applicationName);

            var shareFolderArg = new ShareFolderArg($"{input.Path.TrimEnd('/')}");
            dropBoxClient.Sharing.ShareFolderAsync(shareFolderArg);

        }

        [Action("Download file", Description = "Download specified file")]
        public async Task<byte[]> DownloadFile(string applicationName, AuthenticationCredentialsProvider authenticationCredentialsProvider,
            [ActionParameter] DownlodFileRequest input)
        {
            var dropBoxClient = CreateDropboxClient(authenticationCredentialsProvider.Value, applicationName);

            var downloadArg = new DownloadArg($"{input.Path.TrimEnd('/')}/{input.FileName}");
            using (var response = await dropBoxClient.Files.DownloadAsync(downloadArg))
            {
                byte[] result = await response.GetContentAsByteArrayAsync();
                return result;
            }

        }

        [Action("Get link for file download", Description = "Get temporray link for download of a file")]
        public GetDownloadLinkResponse GetDownloadLink(string applicationName, AuthenticationCredentialsProvider authenticationCredentialsProvider,
            [ActionParameter] DownlodFileRequest input)
        {
            var dropBoxClient = CreateDropboxClient(authenticationCredentialsProvider.Value, applicationName);

            var getLinkArg = new GetTemporaryLinkArg($"{input.Path.TrimEnd('/')}/{input.FileName}");
            var result = dropBoxClient.Files.GetTemporaryLinkAsync(getLinkArg).Result;


            return new GetDownloadLinkResponse()
            {
                LinkForDownload = result.Link,
                Path = result.Metadata.PathDisplay,
                Size = result.Metadata.Size
            };

        }

        private DropboxClient CreateDropboxClient(string accessKey, string appName)
        {
            var httpClient = new HttpClient()
            {
                Timeout = TimeSpan.FromMinutes(20)
            };
            var config = new DropboxClientConfig(appName)
            {
                HttpClient = httpClient
            };

            var client = new DropboxClient(accessKey, config);
            return client;
        }
    }
}
