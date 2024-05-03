using Apps.Dropbox.DataSourceHandlers;
using Apps.Dropbox.Models.Responses.Team;
using Apps.Dropbox.Models.Responses.Team.List;
using Apps.Dropbox.Models.Responses.Team.Sessions;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Dropbox.Api.Team;

namespace Apps.Dropbox.Actions;

public class TeamActions
{
    #region List

    [Action("List members", Description = "Lists members of a team")]
    public async Task<ListMembersResponse> ListMembers(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var dropBoxClient = DropboxClientFactory.CreateDropboxTeamClient(authenticationCredentialsProviders);
        var membersResponse = await dropBoxClient.Team.MembersListAsync();
        var members = membersResponse.Members.Select(x => new MemberResponse(x)).ToList();

        if (!membersResponse.HasMore)
            return new(members);

        var cursor = membersResponse.Cursor;

        do
        {
            membersResponse = await dropBoxClient.Team.MembersListContinueAsync(cursor);
            cursor = membersResponse.Cursor;
            members.AddRange(membersResponse.Members.Select(x => new MemberResponse(x)));
        } while (membersResponse.HasMore);

        return new(members);
    }

    [Action("List member devices", Description = "Lists all device sessions of a team's member")]
    public async Task<MemberDevicesResponse> ListMemberDevices(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] [Display("Team member")] [DataSource(typeof(TeamMemberDataSourceHandler))] string teamMemberId)
    {
        var dropBoxClient = DropboxClientFactory.CreateDropboxTeamClient(authenticationCredentialsProviders);
        var response = await dropBoxClient.Team.DevicesListMemberDevicesAsync(teamMemberId);

        return new()
        {
            TeamMemberId = teamMemberId,
            ActiveWebSessions = response.ActiveWebSessions
                .Select(x => new ActiveSessionResponse(x))
                .ToArray(),
            DesktopClientSessions = response.DesktopClientSessions
                .Select(x => new DesktopSessionResponse(x))
                .ToArray(),
            MobileClientSessions = response.MobileClientSessions
                .Select(x => new MobileSessionResponse(x))
                .ToArray(),
        };
    }

    [Action("List members devices", Description = "Lists all device sessions of a team")]
    public async Task<TeamDevicesResponse> ListMembersDevices(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var dropBoxClient = DropboxClientFactory.CreateDropboxTeamClient(authenticationCredentialsProviders);
        var devices = new List<MemberDevices>();
        ListMembersDevicesResult response;
        string? cursor = null;

        do
        {
            response = await dropBoxClient.Team.DevicesListMembersDevicesAsync(cursor);
            cursor = response.Cursor;
            devices.AddRange(response.Devices);
        } while (response.HasMore);

        return new()
        {
            Devices = devices.Select(x => new MemberDevicesResponse(x)).ToList()
        };
    }

    [Action("List groups", Description = "Lists groups on a team")]
    public async Task<ListGroupsResponse> ListGroups(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var dropBoxClient = DropboxClientFactory.CreateDropboxTeamClient(authenticationCredentialsProviders);
        var groupsResponse = await dropBoxClient.Team.GroupsListAsync();
        var groups = groupsResponse.Groups.Select(x => new GroupResponse(x)).ToList();

        if (!groupsResponse.HasMore)
            return new(groups);

        var cursor = groupsResponse.Cursor;

        do
        {
            groupsResponse = await dropBoxClient.Team.GroupsListContinueAsync(cursor);
            cursor = groupsResponse.Cursor;
            groups.AddRange(groupsResponse.Groups.Select(x => new GroupResponse(x)));
        } while (groupsResponse.HasMore);

        return new(groups);
    }

    [Action("List namespaces", Description = "Lists all team-accessible namespaces.")]
    public async Task<ListNamespacesResponse> ListNamespaces(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var dropBoxClient = DropboxClientFactory.CreateDropboxTeamClient(authenticationCredentialsProviders);
        var namespacesResponse = await dropBoxClient.Team.NamespacesListAsync();
        var namespaces = namespacesResponse.Namespaces.Select(x => new NamespaceResponse(x)).ToList();

        if (!namespacesResponse.HasMore)
            return new(namespaces);

        var cursor = namespacesResponse.Cursor;

        do
        {
            namespacesResponse = await dropBoxClient.Team.NamespacesListContinueAsync(cursor);
            cursor = namespacesResponse.Cursor;
            namespaces.AddRange(namespacesResponse.Namespaces.Select(x => new NamespaceResponse(x)));
        } while (namespacesResponse.HasMore);

        return new(namespaces);
    }

    [Action("List member linked apps", Description = "Lists all linked applications of the team member")]
    public async Task<ListMemberLinkedAppsResponse> ListMemberLinkedApps(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] [Display("Team member")] [DataSource(typeof(TeamMemberDataSourceHandler))] string teamMemberId)
    {
        var dropBoxClient = DropboxClientFactory.CreateDropboxTeamClient(authenticationCredentialsProviders);
        var response = await dropBoxClient.Team.LinkedAppsListMemberLinkedAppsAsync(teamMemberId);
        return new(response.LinkedApiApps.Select(x => new AppResponse(x)).ToList());
    }
    
    [Action("List sharing allowlist", Description = "Lists Approve List entries for given team")]
    public async Task<SharingAllowlist> ListSharingAllowlist(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var dropBoxClient = DropboxClientFactory.CreateDropboxTeamClient(authenticationCredentialsProviders);
        var result = new SharingAllowlist();
        var response = await dropBoxClient.Team.SharingAllowlistListAsync();
        
        result.Domains.AddRange(response.Domains);
        result.Emails.AddRange(response.Emails);

        if (!response.HasMore)
            return result;

        var cursor = response.Cursor;

        do
        {
            response = await dropBoxClient.Team.SharingAllowlistListContinueAsync(cursor);
            cursor = response.Cursor;
            result.Domains.AddRange(response.Domains);
            result.Emails.AddRange(response.Emails);
        } while (response.HasMore);

        return result;
    }

    #endregion

    #region Get

    [Action("Get team info", Description = "Retrieves information about a team")]
    public async Task<TeamResponse> GetTeamInfo(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var dropBoxClient = DropboxClientFactory.CreateDropboxTeamClient(authenticationCredentialsProviders);
        var response = await dropBoxClient.Team.GetInfoAsync();

        return new()
        {
            Name = response.Name,
            TeamId = response.TeamId,
            NumProvisionedUsers = response.NumProvisionedUsers,
            NumUsedLicenses = response.NumUsedLicenses,
            NumLicensedUsers = response.NumLicensedUsers
        };
    }

    [Action("Get group info", Description = "Retrieves information about a group")]
    public async Task<GroupResponse> GetGroupInfo(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] [Display("Group")] [DataSource(typeof(GroupDataSourceHandler))] string groupId)
    {
        var dropBoxClient = DropboxClientFactory.CreateDropboxTeamClient(authenticationCredentialsProviders);
        var response = await dropBoxClient.Team.GroupsGetInfoAsync(new GroupsSelector.GroupIds(new[] { groupId }));
        var group = response.First();

        if (group.IsIdNotFound)
            throw new Exception($"No group found with {groupId} id");

        return new(group.AsGroupInfo.Value);
    }

    #endregion

    #region Create

    [Action("Create group", Description = "Creates a new, empty group")]
    public async Task<GroupResponse> CreateGroup(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] [Display("Group name")] string name)
    {
        var dropBoxClient = DropboxClientFactory.CreateDropboxTeamClient(authenticationCredentialsProviders);
        var response = await dropBoxClient.Team.GroupsCreateAsync(name);
        return new(response);
    }

    #endregion

    #region Delete

    [Action("Delete group", Description = "Deletes a group")]
    public Task DeleteGroup(
        IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders,
        [ActionParameter] [Display("Group")] [DataSource(typeof(GroupDataSourceHandler))] string groupId)
    {
        var dropBoxClient = DropboxClientFactory.CreateDropboxTeamClient(authenticationCredentialsProviders);
        return dropBoxClient.Team.GroupsDeleteAsync(new GroupSelector.GroupId(groupId));
    }

    #endregion
}