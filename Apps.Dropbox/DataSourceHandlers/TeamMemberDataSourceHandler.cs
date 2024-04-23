using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Dropbox.DataSourceHandlers;

public class TeamMemberDataSourceHandler(InvocationContext invocationContext)
    : BaseInvocable(invocationContext), IAsyncDataSourceHandler
{
    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        return await ErrorWrapper.WrapError(async () =>
        {
            var client =
                DropboxClientFactory.CreateDropboxTeamClient(InvocationContext.AuthenticationCredentialsProviders);
            var members = (await client.Team.MembersListAsync()).Members;
            return members.ToDictionary(m => m.Profile.TeamMemberId, m => m.Profile.Email);
        });
    }
}