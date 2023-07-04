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

    [Display("Member count")] public uint? MemberCount { get; init; }

    [Display("External id")] public string ExternalId { get; init; }

    public string Name { get; init; }

    public string Id { get; init; }
}