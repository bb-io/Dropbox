using Blackbird.Applications.Sdk.Common;
using Dropbox.Api.TeamCommon;

namespace Apps.Dropbox.Models.Responses.Team;

public class GroupResponse
{
    public GroupResponse(GroupSummary group)
    {
        Id = group.GroupId;
        Name = group.GroupName;
        ExternalId = group.GroupExternalId;
        MemberCount = group.MemberCount;
    }

    [Display("Group ID")]
    public string Id { get; init; }
    
    [Display("Member count")] 
    public uint? MemberCount { get; init; }

    [Display("External ID")] 
    public string ExternalId { get; init; }

    [Display("Group name")]
    public string Name { get; init; }
}