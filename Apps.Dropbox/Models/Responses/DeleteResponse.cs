using Blackbird.Applications.Sdk.Common;

namespace Apps.Dropbox.Models.Responses
{
    public class DeleteResponse
    {
        [Display("Deleted object path")]
        public string DeletedObjectPath { get; set; }
    }
}
