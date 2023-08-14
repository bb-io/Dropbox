using Apps.Dropbox.Models.Responses.Team.Sessions.Base;
using Blackbird.Applications.Sdk.Common;
using Dropbox.Api.Team;

namespace Apps.Dropbox.Models.Responses.Team.Sessions;

public class MobileSessionResponse : DeviceSessionResponse
{
    public MobileSessionResponse(MobileClientSession session) : base(session)
    {
        DeviceName = session.DeviceName;
        ClientVersion = session.ClientVersion;
        OsVersion = session.OsVersion;
    }

    [Display("Client version")] 
    public string ClientVersion { get; set; }

    [Display("OS version")] 
    public string OsVersion { get; set; }

    [Display("Device name")] 
    public string DeviceName { get; set; }
}