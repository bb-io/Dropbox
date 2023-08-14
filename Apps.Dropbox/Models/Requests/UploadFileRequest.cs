using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Dropbox.Models.Requests
{
    public class UploadFileRequest
    {
        [Display("Parent folder path")]
        [DataSource(typeof(FolderDataSourceHandler))]
        public string ParentFolderPath { get; set; }

        public string Filename { get; set; }

        public byte[] File { get; set; }
    }
}
