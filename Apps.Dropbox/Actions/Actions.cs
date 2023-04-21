using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Dropbox.Models.Responses;
using Apps.Dropbox.Models.Requests;
using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.FileRequests;
using Dropbox.Api.Sharing;
using Blackbird.Applications.Sdk.Common.Actions;

namespace Apps.Dropbox.Actions
{
    [ActionList]
    public class Actions
    {
        [Action("Get folders list by path", Description = "Get folders list by specified path")]
        public FoldersResponse GetFoldersListByPath(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] FoldersRequest input)
        {
            var accessToken = authenticationCredentialsProviders.First(p => p.KeyName == "accessToken");
            var applicationName = authenticationCredentialsProviders.First(p => p.KeyName == "applicationName");

            var dropBoxClient = CreateDropboxClient(accessToken.Value, applicationName.Value);
            
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
        public FilesResponse GetFilesListByPath(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] FilesRequest input)
        {
            var accessToken = authenticationCredentialsProviders.First(p => p.KeyName == "accessToken");
            var applicationName = authenticationCredentialsProviders.First(p => p.KeyName == "applicationName");

            var dropBoxClient = CreateDropboxClient(accessToken.Value, applicationName.Value);

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
        public CreateFolderResponse CreateFolder(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] CreateFolderRequest input)
        {
            var accessToken = authenticationCredentialsProviders.First(p => p.KeyName == "accessToken");
            var applicationName = authenticationCredentialsProviders.First(p => p.KeyName == "applicationName");

            var dropBoxClient = CreateDropboxClient(accessToken.Value, applicationName.Value);

            var folderArg = new CreateFolderArg($"{input.Path.TrimEnd('/')}/{input.FolderName}");
            var result = dropBoxClient.Files.CreateFolderV2Async(folderArg).Result;


            return new CreateFolderResponse()
            {
                FolderPath = result.Metadata.PathDisplay
            };
        }

        [Action("Upload file", Description = "Upload file")]
        public BaseResponse UploadFile(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] UploadFileRequest input)
        {
            var accessToken = authenticationCredentialsProviders.First(p => p.KeyName == "accessToken");
            var applicationName = authenticationCredentialsProviders.First(p => p.KeyName == "applicationName");

            var dropBoxClient = CreateDropboxClient(accessToken.Value, applicationName.Value);

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
        public DeleteResponse DeleteFolder(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] DeleteRequest input)
        {
            var accessToken = authenticationCredentialsProviders.First(p => p.KeyName == "accessToken");
            var applicationName = authenticationCredentialsProviders.First(p => p.KeyName == "applicationName");

            var dropBoxClient = CreateDropboxClient(accessToken.Value, applicationName.Value);

            var objectArg = new DeleteArg($"{input.Path.TrimEnd('/')}/{input.ObjectToDeleteName}");
            var result = dropBoxClient.Files.DeleteV2Async(objectArg).Result;


            return new DeleteResponse()
            {
                DeletedObjectPath = result.Metadata.PathDisplay
            };
        }

        [Action("Move file", Description = "Move file from one directory to another")]
        public MoveFileResponse MoveFile(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] MoveFileRequest input)
        {
            var accessToken = authenticationCredentialsProviders.First(p => p.KeyName == "accessToken");
            var applicationName = authenticationCredentialsProviders.First(p => p.KeyName == "applicationName");

            var dropBoxClient = CreateDropboxClient(accessToken.Value, applicationName.Value);

            var moveArg = new RelocationArg($"{input.PathFrom.TrimEnd('/')}/{input.SourceFileName}", $"{input.PathTo.TrimEnd('/')}/{input.TargetFileName}");
            var result = dropBoxClient.Files.MoveV2Async(moveArg).Result;

            return new MoveFileResponse()
            {
                FileName = result.Metadata.Name,
                NewFilePath = result.Metadata.PathDisplay
            };
        }

        [Action("Copy file", Description = "Copy file from one directory to another")]
        public MoveFileResponse CopyFile(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] MoveFileRequest input)
        {
            var accessToken = authenticationCredentialsProviders.First(p => p.KeyName == "accessToken");
            var applicationName = authenticationCredentialsProviders.First(p => p.KeyName == "applicationName");

            var dropBoxClient = CreateDropboxClient(accessToken.Value, applicationName.Value);

            var copyArg = new RelocationArg($"{input.PathFrom.TrimEnd('/')}/{input.SourceFileName}", $"{input.PathTo.TrimEnd('/')}/{input.TargetFileName}");
            var result = dropBoxClient.Files.CopyV2Async(copyArg).Result;

            return new MoveFileResponse()
            {
                FileName = result.Metadata.Name,
                NewFilePath = result.Metadata.PathDisplay
            };
        }

        [Action("Create file request", Description = "Create file request for current user")]
        public CreateFileRequestResponse CreateFileRequest(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] CreateFileRequestRequest input)
        {
            var accessToken = authenticationCredentialsProviders.First(p => p.KeyName == "accessToken");
            var applicationName = authenticationCredentialsProviders.First(p => p.KeyName == "applicationName");

            var dropBoxClient = CreateDropboxClient(accessToken.Value, applicationName.Value);

            var createFileArg = new CreateFileRequestArgs(input.RequestTitle,input.Destination);
            var result = dropBoxClient.FileRequests.CreateAsync(createFileArg).Result;


            return new CreateFileRequestResponse()
            {
                RequestUrl = result.Url,
                Destination = result.Destination
            };
        }

        [Action("Share folder", Description = "Share given folder")]
        public void ShareFolder(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] ShareFolderRequest input)
        {
            var accessToken = authenticationCredentialsProviders.First(p => p.KeyName == "accessToken");
            var applicationName = authenticationCredentialsProviders.First(p => p.KeyName == "applicationName");

            var dropBoxClient = CreateDropboxClient(accessToken.Value, applicationName.Value);

            var shareFolderArg = new ShareFolderArg($"{input.Path.TrimEnd('/')}");
            dropBoxClient.Sharing.ShareFolderAsync(shareFolderArg);

        }

        [Action("Download file", Description = "Download specified file")]
        public async Task<byte[]> DownloadFile(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] DownlodFileRequest input)
        {
            var accessToken = authenticationCredentialsProviders.First(p => p.KeyName == "accessToken");
            var applicationName = authenticationCredentialsProviders.First(p => p.KeyName == "applicationName");

            var dropBoxClient = CreateDropboxClient(accessToken.Value, applicationName.Value);

            var downloadArg = new DownloadArg($"{input.Path.TrimEnd('/')}/{input.FileName}");
            using (var response = await dropBoxClient.Files.DownloadAsync(downloadArg))
            {
                byte[] result = await response.GetContentAsByteArrayAsync();
                return result;
            }

        }

        [Action("Get link for file download", Description = "Get temporray link for download of a file")]
        public GetDownloadLinkResponse GetDownloadLink(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] DownlodFileRequest input)
        {
            var accessToken = authenticationCredentialsProviders.First(p => p.KeyName == "accessToken");
            var applicationName = authenticationCredentialsProviders.First(p => p.KeyName == "applicationName");

            var dropBoxClient = CreateDropboxClient(accessToken.Value, applicationName.Value);

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
