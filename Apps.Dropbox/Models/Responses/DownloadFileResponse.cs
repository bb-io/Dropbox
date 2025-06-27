using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.FileStorage;

namespace Apps.Dropbox.Models.Responses;

public class DownloadFileResponse : IDownloadFileOutput
{
    public FileReference File { get; set; }
}