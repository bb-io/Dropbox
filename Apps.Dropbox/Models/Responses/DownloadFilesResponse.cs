using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Dropbox.Models.Responses
{
    public class DownloadFilesResponse
    {
        public IEnumerable<FileReference> Files { get; set; }
    }
}
