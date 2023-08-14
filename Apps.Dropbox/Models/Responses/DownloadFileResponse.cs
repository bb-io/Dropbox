namespace Apps.Dropbox.Models.Responses;

public class DownloadFileResponse
{
    public string Filename { get; set; }
    public byte[] File { get; set; }
}