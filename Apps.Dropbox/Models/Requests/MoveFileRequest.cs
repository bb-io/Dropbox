using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Dropbox.Models.Requests
{
    public class MoveFileRequest
    {
        [DataSource(typeof(FileDataSourceHandler))]
        public string CurrentFilePath { get; set; }

        [DataSource(typeof(FolderDataSourceHandler))]
        public string DestinationFolder { get; set; }

        public string? TargetFilename { get; set; }
    }
}
