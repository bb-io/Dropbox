using Apps.Dropbox.Models.Responses.Team.Sessions;
using Blackbird.Applications.Sdk.Common;
using Dropbox.Api.Team;

namespace Apps.Dropbox.Models.Responses.Team.List;

public class MemberDevicesResponse
{
    public MemberDevicesResponse()
    {
    }

    public MemberDevicesResponse(MemberDevices memberDevices)
    {
        TeamMemberId = memberDevices.TeamMemberId;
        ActiveWebSessions = memberDevices.WebSessions.Select(x => new ActiveSessionResponse(x)).ToArray();
        DesktopClientSessions = memberDevices.DesktopClients.Select(x => new DesktopSessionResponse(x)).ToArray();
        MobileClientSessions = memberDevices.MobileClients.Select(x => new MobileSessionResponse(x)).ToArray();
    }

    [Display("Team member id")] public string TeamMemberId { get; init; }

    [Display("Active web sessions")] public ActiveSessionResponse[] ActiveWebSessions { get; init; }

    [Display("Desktop client sessions")] public DesktopSessionResponse[] DesktopClientSessions { get; init; }

    [Display("Mobile client sessions")] public MobileSessionResponse[] MobileClientSessions { get; init; }
}