namespace Apps.Dropbox.Models.Requests
{
    public class MoveFileRequest
    {
        public string PathFrom { get; set; }

        public string SourceFileName { get; set; }

        public string PathTo { get; set; }

        public string TargetFileName { get; set; }
    }
}
