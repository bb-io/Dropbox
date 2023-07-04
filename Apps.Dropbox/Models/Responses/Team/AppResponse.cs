using Blackbird.Applications.Sdk.Common;
using Dropbox.Api.Team;

namespace Apps.Dropbox.Models.Responses.Team;

public class AppResponse
{
    public AppResponse(ApiApp app)
    {
        Id = app.AppId;
        Name = app.AppName;
        IsAppFolder = app.IsAppFolder;
        Publisher = app.Publisher;
    }

    public string Id { get; set; }
    public string Publisher { get; set; }
    [Display("Is app folder?")]public bool IsAppFolder { get; set; }
    public string Name { get; set; }
}