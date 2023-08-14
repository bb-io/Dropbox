using Blackbird.Applications.Sdk.Common;
using Dropbox.Api.Team;

namespace Apps.Dropbox.Models.Responses.Team.Sessions.Base;

public class DeviceSessionResponse
{
    public DeviceSessionResponse(DeviceSession session)
    {
        SessionId = session.SessionId;
        IpAddress = session.IpAddress;
        Created = session.Created;
    }

    public DateTime? Created { get; set; }

    [Display("IP address")] 
    public string IpAddress { get; set; }

    [Display("Session ID")] 
    public string SessionId { get; set; }
}