using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Dropbox.Models.Requests
{
    public class DownloadFolderRequest
    {
        [Display("Folder path")]
        [DataSource(typeof(FolderDataSourceHandler))]
        public string FolderPath { get; set; }
    }
}
