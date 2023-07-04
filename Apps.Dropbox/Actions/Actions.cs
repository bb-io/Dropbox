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
        public async Task<FoldersResponse> GetFoldersListByPath(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] FoldersRequest input)
        {
            var dropBoxClient = CreateDropboxClient(authenticationCredentialsProviders);
            
            string foldersNames = "";
            var list = await dropBoxClient.Files.ListFolderAsync(input.Path);
            foreach (var item in list.Entries.Where(i => i.IsFolder))
            {
                foldersNames += item.Name + ", ";
            }
            return new FoldersResponse
            {
                FolderNames = foldersNames
            };
        }

        [Action("Get files list by path", Description = "Get files list by specified path")]
        public async Task<FilesResponse> GetFilesListByPath(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] FilesRequest input)
        {
            var dropBoxClient = CreateDropboxClient(authenticationCredentialsProviders);

            string filesNames = "";
            var list = await dropBoxClient.Files.ListFolderAsync(input.Path);
            foreach (var item in list.Entries.Where(i => i.IsFile))
            {
                filesNames += item.Name + ", ";
            }
            return new FilesResponse
            {
                FileNames = filesNames
            };
        }

        [Action("Create folder", Description = "Create folder with a given name")]
        public async Task<CreateFolderResponse> CreateFolder(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] CreateFolderRequest input)
        {
            var dropBoxClient = CreateDropboxClient(authenticationCredentialsProviders);

            var folderArg = new CreateFolderArg($"{input.Path.TrimEnd('/')}/{input.FolderName}");
            var result = await dropBoxClient.Files.CreateFolderV2Async(folderArg);


            return new CreateFolderResponse
            {
                FolderPath = result.Metadata.PathDisplay
            };
        }

        [Action("Upload file", Description = "Upload file")]
        public async Task<FileUploadResponse> UploadFile(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] UploadFileRequest input)
        {
            var dropBoxClient = CreateDropboxClient(authenticationCredentialsProviders);

            using (var stream = new MemoryStream(input.File)) 
            {
                var response = await dropBoxClient.Files.UploadAsync($"{input.Path.TrimEnd('/')}/{input.Filename}.{input.FileType}", WriteMode.Overwrite.Instance, body: stream);

                return new FileUploadResponse
                {
                    Id = response.Id
                };
            } 
        }

        [Action("Delete", Description = "Delete specified folder or file")]
        public async Task<DeleteResponse> DeleteFolder(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] DeleteRequest input)
        {
            var dropBoxClient = CreateDropboxClient(authenticationCredentialsProviders);

            var objectArg = new DeleteArg($"{input.Path.TrimEnd('/')}/{input.ObjectToDeleteName}");
            var result = await dropBoxClient.Files.DeleteV2Async(objectArg);


            return new DeleteResponse
            {
                DeletedObjectPath = result.Metadata.PathDisplay
            };
        }

        [Action("Move file", Description = "Move file from one directory to another")]
        public async Task<MoveFileResponse> MoveFile(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] MoveFileRequest input)
        {
            var dropBoxClient = CreateDropboxClient(authenticationCredentialsProviders);

            var moveArg = new RelocationArg($"{input.PathFrom.TrimEnd('/')}/{input.SourceFileName}", $"{input.PathTo.TrimEnd('/')}/{input.TargetFileName}");
            var result = await dropBoxClient.Files.MoveV2Async(moveArg);

            return new MoveFileResponse
            {
                FileName = result.Metadata.Name,
                NewFilePath = result.Metadata.PathDisplay
            };
        }

        [Action("Copy file", Description = "Copy file from one directory to another")]
        public async Task<MoveFileResponse> CopyFile(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] MoveFileRequest input)
        {
            var dropBoxClient = CreateDropboxClient(authenticationCredentialsProviders);

            var copyArg = new RelocationArg($"{input.PathFrom.TrimEnd('/')}/{input.SourceFileName}", $"{input.PathTo.TrimEnd('/')}/{input.TargetFileName}");
            var result = await dropBoxClient.Files.CopyV2Async(copyArg);

            return new MoveFileResponse
            {
                FileName = result.Metadata.Name,
                NewFilePath = result.Metadata.PathDisplay
            };
        }

        [Action("Create file request", Description = "Create file request for current user")]
        public async Task<CreateFileRequestResponse> CreateFileRequest(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] CreateFileRequestRequest input)
        {
            var dropBoxClient = CreateDropboxClient(authenticationCredentialsProviders);

            var createFileArg = new CreateFileRequestArgs(input.RequestTitle,input.Destination);
            var result = await dropBoxClient.FileRequests.CreateAsync(createFileArg);


            return new CreateFileRequestResponse
            {
                RequestUrl = result.Url,
                Destination = result.Destination
            };
        }

        [Action("Share folder", Description = "Share given folder")]
        public void ShareFolder(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] ShareFolderRequest input)
        {
            var dropBoxClient = CreateDropboxClient(authenticationCredentialsProviders);

            var shareFolderArg = new ShareFolderArg($"{input.Path.TrimEnd('/')}");
            dropBoxClient.Sharing.ShareFolderAsync(shareFolderArg);
        }

        [Action("Download file", Description = "Download specified file")]
        public async Task<byte[]> DownloadFile(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
            [ActionParameter] DownlodFileRequest input)
        {
            var dropBoxClient = CreateDropboxClient(authenticationCredentialsProviders);

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
            var dropBoxClient = CreateDropboxClient(authenticationCredentialsProviders);

            var getLinkArg = new GetTemporaryLinkArg($"{input.Path.TrimEnd('/')}/{input.FileName}");
            var result = dropBoxClient.Files.GetTemporaryLinkAsync(getLinkArg).Result;


            return new GetDownloadLinkResponse
            {
                LinkForDownload = result.Link,
                Path = result.Metadata.PathDisplay,
                Size = result.Metadata.Size
            };

        }

        private DropboxClient CreateDropboxClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
        {
            var accessToken = authenticationCredentialsProviders.First(p => p.KeyName == "accessToken").Value;
            var applicationName = authenticationCredentialsProviders.First(p => p.KeyName == "applicationName").Value;

            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(20)
            };
            var config = new DropboxClientConfig(applicationName)
            {
                HttpClient = httpClient
            };

            var client = new DropboxClient(accessToken, config);
            return client;
        }
    }
}
