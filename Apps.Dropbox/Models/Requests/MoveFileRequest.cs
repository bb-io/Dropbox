using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Dropbox.Models.Requests
{
    public class MoveFileRequest
    {
        [Display("Current file path")]
        [DataSource(typeof(FileDataSourceHandler))]
        public string CurrentFilePath { get; set; }

        [Display("Destination folder")]
        [DataSource(typeof(FolderDataSourceHandler))]
        public string DestinationFolder { get; set; }

        [Display("Target filename")]
        public string? TargetFilename { get; set; }
    }
}
