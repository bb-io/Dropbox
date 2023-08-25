using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.Dropbox.Models.Responses;

public class DownloadFileResponse
{
    public File File { get; set; }
}