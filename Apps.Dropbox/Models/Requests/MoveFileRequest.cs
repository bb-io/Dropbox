using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.Dropbox.Models.Requests
{
    public class MoveFileRequest
    {
        [Display("Current file path")]
        [FileDataSource(typeof(FilePickerDataSourceHandler))]
        public string CurrentFilePath { get; set; }

        [Display("Destination folder")]
        [DataSource(typeof(FolderDataSourceHandler))]
        public string DestinationFolder { get; set; }

        [Display("Target filename")]
        public string? TargetFilename { get; set; }
    }
}
