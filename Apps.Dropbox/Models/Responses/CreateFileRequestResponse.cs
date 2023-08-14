using Blackbird.Applications.Sdk.Common;

namespace Apps.Dropbox.Models.Responses
{
    public class CreateFileRequestResponse
    {
        [Display("Request URL")]
        public string RequestUrl { get; set; }
        
        public string Destination { get; set; }
    }
}
