using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Blueprints.Interfaces.FileStorage;

namespace Apps.Dropbox.Models.Requests
{
    public class UploadFileRequest : IUploadFileInput
    {
        [Display("Parent folder path")]
        [DataSource(typeof(FolderDataSourceHandler))]
        public string? ParentFolderPath { get; set; }

        public FileReference File { get; set; }
    }
}
