using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Dropbox.DataSourceHandlers;

public class TeamMemberDataSourceHandler : BaseInvocable, IAsyncDataSourceHandler
{
    public TeamMemberDataSourceHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = DropboxClientFactory.CreateDropboxTeamClient(InvocationContext.AuthenticationCredentialsProviders);
        var teamMembers = (await client.Team.MembersListAsync()).Members;
        return teamMembers.ToDictionary(m => m.Profile.TeamMemberId, m => m.Profile.Name.DisplayName);
    }
}