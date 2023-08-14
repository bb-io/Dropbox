using Apps.Dropbox.Models.Responses.Team.Sessions.Base;
using Blackbird.Applications.Sdk.Common;
using Dropbox.Api.Team;

namespace Apps.Dropbox.Models.Responses.Team.Sessions;

public class DesktopSessionResponse : DeviceSessionResponse
{
    public DesktopSessionResponse(DesktopClientSession session) : base(session)
    {
        HostName = session.HostName;
        ClientVersion = session.ClientVersion;
        Platform = session.Platform;
    }

    public string Platform { get; set; }

    [Display("Client version")] 
    public string ClientVersion { get; set; }

    [Display("Host name")] 
    public string HostName { get; set; }
}