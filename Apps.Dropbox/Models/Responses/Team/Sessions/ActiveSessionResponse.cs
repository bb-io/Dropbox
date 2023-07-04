using Apps.Dropbox.Models.Responses.Team.Sessions.Base;
using Blackbird.Applications.Sdk.Common;
using Dropbox.Api.Team;

namespace Apps.Dropbox.Models.Responses.Team.Sessions;

public class ActiveSessionResponse : DeviceSessionResponse
{
    public ActiveSessionResponse(ActiveWebSession session) : base(session)
    {
        Os = session.Os;
        Browser = session.Browser;
        UserAgent = session.UserAgent;
    }

    public string Os { get; set; }

    public string Browser { get; set; }

    [Display("User agent")] public string UserAgent { get; set; }
}