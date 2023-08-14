using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Dropbox.Models.Requests
{
    public class CreateFolderRequest
    {
        [Display("Parent folder path")]
        [DataSource(typeof(FolderDataSourceHandler))]
        public string ParentFolderPath { get; set; }

        [Display("Folder name")]
        public string FolderName { get; set; }
    }
}
