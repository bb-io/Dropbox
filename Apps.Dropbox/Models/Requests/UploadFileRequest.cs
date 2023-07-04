namespace Apps.Dropbox.Models.Requests
{
    public class UploadFileRequest
    {
        public string Path { get; set; }

        public string Filename { get; set; }

        public string FileType { get; set; }

        public byte[] File { get; set; }
    }
}
