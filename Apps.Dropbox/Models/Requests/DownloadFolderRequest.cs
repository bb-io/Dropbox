using Apps.Dropbox.DataSourceHandlers;
using Apps.Dropbox.DataSourceHandlers.Enum;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.Dropbox.Models.Requests
{
    public class DownloadFolderRequest
    {
        [Display("Folder path")]
        [FileDataSource(typeof(FolderPickerDataSourceHandler))]
        public string FolderPath { get; set; }

        [Display("Include files from subfolders", Description = "Choose the folder`s level of downloading files")]
        [StaticDataSource(typeof(SubfolderDataHandler))]
        public string? SubfolderScope { get; set; } = "none";
    }
}
