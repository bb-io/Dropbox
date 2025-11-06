using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.Dropbox.Models.Requests
{
    public class CreateFileRequestRequest
    {
        [Display("Request title")]
        public string RequestTitle { get; set; }

        [FileDataSource(typeof(FolderPickerDataSourceHandler))]
        public string Destination { get; set; }
    }
}
