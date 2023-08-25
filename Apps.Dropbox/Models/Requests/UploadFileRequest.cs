using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.Dropbox.Models.Requests
{
    public class UploadFileRequest
    {
        [Display("Parent folder path")]
        [DataSource(typeof(FolderDataSourceHandler))]
        public string ParentFolderPath { get; set; }

        public File File { get; set; }
    }
}
