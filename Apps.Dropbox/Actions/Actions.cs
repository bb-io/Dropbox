using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Apps.Dropbox.Models.Responses;
using Apps.Dropbox.Models.Requests;
using Dropbox.Api;
using Dropbox.Api.Common;
using Dropbox.Api.Files;
using Dropbox.Api.Team;
using System.Net.Http;

namespace Apps.Zendesk.Actions
{
    [ActionList]
    public class Actions
    {
        [Action]
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

        [Action]
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

        [Action]
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
