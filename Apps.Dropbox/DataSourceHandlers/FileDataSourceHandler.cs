using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace Apps.Dropbox.DataSourceHandlers;

public class FileDataSourceHandler : BaseInvocable, IAsyncDataSourceHandler
{
    public FileDataSourceHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = DropboxClientFactory.CreateDropboxClient(InvocationContext.AuthenticationCredentialsProviders);
        IEnumerable<Metadata> files;

        if (string.IsNullOrWhiteSpace(context.SearchString))
            files = await ListFiles(client);
        else
            files = await ListFiles(client, context.SearchString);

        var filesDictionary = files.ToDictionary(f => f.PathLower, f => f.PathDisplay);

        foreach (var file in filesDictionary)
        {
            var displayFilePath = file.Value;
            if (displayFilePath.Length > 40)
            {
                var filePathParts = displayFilePath.Split("/");
                if (filePathParts.Length > 4)
                {
                    displayFilePath = string.Join("/", "", filePathParts[1], "...", filePathParts[^2], filePathParts[^1]);
                    filesDictionary[file.Key] = displayFilePath;
                }
            }
        }
        
        var currentAccount = await client.Users.GetCurrentAccountAsync();
        var accountId = currentAccount.AccountId;
        
        await Logger.LogAsync(new
        {
            AccountId = accountId,
        });

        return filesDictionary;
    }

    private async Task<IEnumerable<Metadata>> ListFiles(DropboxClient client)
    {
        var files = new List<Metadata>();
        var result = await client.Files.ListFolderAsync("", recursive: true, limit: 20);
        var resultFiles = result.Entries.Where(e => e.IsFile);
        files.AddRange(resultFiles);
        
        while (result.HasMore && files.Count < 20)
        {
            result = await client.Files.ListFolderContinueAsync(result.Cursor);
            resultFiles = result.Entries.Where(e => e.IsFile);
            files.AddRange(resultFiles);
        }

        return files;
    }
    
    private async Task<IEnumerable<Metadata>> ListFiles(DropboxClient client, string searchString)
    {
        var files = new List<Metadata>();
        var result = await client.Files.SearchV2Async(searchString, new SearchOptions(maxResults: 20));
        var resultFiles = result.Matches.Select(m => m.Metadata.AsMetadata.Value).Where(m => m.IsFile);
        files.AddRange(resultFiles);

        while (result.HasMore && files.Count < 20)
        {
            result = await client.Files.SearchContinueV2Async(result.Cursor);
            resultFiles = result.Matches.Select(m => m.Metadata.AsMetadata.Value).Where(m => m.IsFile);
            files.AddRange(resultFiles);
        }
        
        return files;
    }
}