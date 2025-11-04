using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Blueprints.Interfaces.FileStorage;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.Dropbox.Models.Requests
{
    public class DownloadFileRequest : IDownloadFileInput
    {
        [Display("File path")]
        [FileDataSource(typeof(FilePickerDataSourceHandler))]
        public string FileId { get; set; }
    }
}
