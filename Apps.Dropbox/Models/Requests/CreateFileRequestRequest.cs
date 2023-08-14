using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Dropbox.Models.Requests
{
    public class CreateFileRequestRequest
    {
        [Display("Request title")]
        public string RequestTitle { get; set; }
        
        [DataSource(typeof(FolderDataSourceHandler))]
        public string Destination { get; set; }
    }
}
