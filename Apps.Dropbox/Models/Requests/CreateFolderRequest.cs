using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.Dropbox.Models.Requests
{
    public class CreateFolderRequest
    {
        [Display("Parent folder path")]
        [FileDataSource(typeof(FolderPickerDataSourceHandler))]
        public string ParentFolderPath { get; set; }

        [Display("Folder name")]
        public string FolderName { get; set; }
    }
}
