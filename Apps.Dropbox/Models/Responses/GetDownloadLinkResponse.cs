using Blackbird.Applications.Sdk.Common;

namespace Apps.Dropbox.Models.Responses
{
    public class GetDownloadLinkResponse
    {
        [Display("Link for download")]
        public string LinkForDownload { get; set; }

        public string Path { get; set; }

        [Display("Size in bytes")]
        public ulong SizeInBytes { get; set; }
    }
}
