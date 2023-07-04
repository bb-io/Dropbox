using Blackbird.Applications.Sdk.Common;
using Dropbox.Api.Team;

namespace Apps.Dropbox.Models.Responses.Team;

public class MemberResponse
{
    public MemberResponse(TeamMemberInfo teamMemberInfo)
    {
        Groups = teamMemberInfo.Profile.Groups;
        MemberFolderId = teamMemberInfo.Profile.MemberFolderId;
        Email = teamMemberInfo.Profile.Email;
        Name = teamMemberInfo.Profile.Name.DisplayName;
        Avatar = teamMemberInfo.Profile.ProfilePhotoUrl;
    }

    public string Avatar { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    [Display("Member folder id")] public string MemberFolderId { get; set; }

    public IList<string> Groups { get; set; }
}