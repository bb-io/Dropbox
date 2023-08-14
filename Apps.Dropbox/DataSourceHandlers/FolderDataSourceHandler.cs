using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace Apps.Dropbox.DataSourceHandlers;

public class FolderDataSourceHandler : BaseInvocable, IAsyncDataSourceHandler
{
    public FolderDataSourceHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = DropboxClientFactory.CreateDropboxClient(InvocationContext.AuthenticationCredentialsProviders);
        IEnumerable<Metadata> folders;

        if (string.IsNullOrWhiteSpace(context.SearchString))
            folders = await ListFolders(client);
        else
            folders = await ListFolders(client, context.SearchString);

        var folderDictionary = folders.ToDictionary(f => f.PathLower, f => f.PathDisplay);
        
        foreach (var folder in folderDictionary)
        {
            var displayFolderPath = folder.Value;
            if (displayFolderPath.Length > 40)
            {
                var folderPathParts = displayFolderPath.Split("/");
                if (folderPathParts.Length > 3)
                {
                    displayFolderPath = string.Join("/", "", folderPathParts[1], "...", folderPathParts[^2], folderPathParts[^1]);
                    folderDictionary[folder.Key] = displayFolderPath;
                }
            }
        }

        return folderDictionary;
    }

    private async Task<IEnumerable<Metadata>> ListFolders(DropboxClient client)
    {
        var folders = new List<Metadata>();
        var result = await client.Files.ListFolderAsync("", recursive: true, limit: 20);
        var resultFolders = result.Entries.Where(e => e.IsFolder);
        folders.AddRange(resultFolders);
        
        while (result.HasMore && folders.Count < 20)
        {
            result = await client.Files.ListFolderContinueAsync(result.Cursor);
            resultFolders = result.Entries.Where(e => e.IsFolder);
            folders.AddRange(resultFolders);
        }

        return folders;
    }
    
    private async Task<IEnumerable<Metadata>> ListFolders(DropboxClient client, string searchString)
    {
        var folders = new List<Metadata>();
        var result = await client.Files.SearchV2Async(searchString, new SearchOptions(maxResults: 20));
        var resultFolders = result.Matches.Select(m => m.Metadata.AsMetadata.Value).Where(m => m.IsFolder);
        folders.AddRange(resultFolders);

        while (result.HasMore && folders.Count < 20)
        {
            result = await client.Files.SearchContinueV2Async(result.Cursor);
            resultFolders = result.Matches.Select(m => m.Metadata.AsMetadata.Value).Where(m => m.IsFolder);
            folders.AddRange(resultFolders);
        }
        
        return folders;
    }
}