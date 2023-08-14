using Blackbird.Applications.Sdk.Common;

namespace Apps.Dropbox.Models.Responses
{
    public class CreateFolderResponse
    {
        [Display("Folder path")]
        public string FolderPath { get; set; }
    }
}
