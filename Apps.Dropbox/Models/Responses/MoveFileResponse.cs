using Blackbird.Applications.Sdk.Common;

namespace Apps.Dropbox.Models.Responses
{
    public class MoveFileResponse
    {
        [Display("Filename")]
        public string FileName { get; set; }

        [Display("New file path")]
        public string NewFilePath { get; set; }
    }
}
