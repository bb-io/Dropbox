using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Dropbox.Models.Requests
{
    public class DownloadFileRequest
    {
        [DataSource(typeof(FileDataSourceHandler))]
        public string FilePath { get; set; }
    }
}
