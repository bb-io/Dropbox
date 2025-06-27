using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Blueprints.Interfaces.FileStorage;

namespace Apps.Dropbox.Models.Requests
{
    public class DownloadFileRequest : IDownloadFileInput
    {
        [Display("File path")]
        [DataSource(typeof(FileDataSourceHandler))]
        public string FileId { get; set; }
    }
}
