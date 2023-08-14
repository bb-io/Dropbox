using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Dropbox.DataSourceHandlers;

public class GroupDataSourceHandler : BaseInvocable, IAsyncDataSourceHandler
{
    public GroupDataSourceHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = DropboxClientFactory.CreateDropboxTeamClient(InvocationContext.AuthenticationCredentialsProviders);
        var groups = (await client.Team.GroupsListAsync()).Groups;
        return groups.ToDictionary(g => g.GroupId, g => g.GroupName);
    }
}