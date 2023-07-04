namespace Apps.Dropbox.Models.Responses
{
    public class GetDownloadLinkResponse
    {
        public string LinkForDownload { get; set; }

        public string Path { get; set; }

        public ulong Size { get; set; }
    }
}
