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

    [Display("App ID")]
    public string Id { get; set; }
    
    public string Publisher { get; set; }
    
    [Display("Is app folder?")]
    public bool IsAppFolder { get; set; }
    
    [Display("App name")]
    public string Name { get; set; }
}