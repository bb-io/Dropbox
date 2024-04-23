using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Dropbox.DataSourceHandlers;

public class GroupDataSourceHandler(InvocationContext invocationContext)
    : BaseInvocable(invocationContext), IAsyncDataSourceHandler
{
    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        return await ErrorWrapper.WrapError(async () =>
        {
            var client = DropboxClientFactory.CreateDropboxTeamClient(InvocationContext.AuthenticationCredentialsProviders);
            var groups = (await client.Team.GroupsListAsync()).Groups;
            return groups.ToDictionary(g => g.GroupId, g => g.GroupName);
        });
    }
}