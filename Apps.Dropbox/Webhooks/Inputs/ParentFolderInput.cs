using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.Dropbox.Webhooks.Inputs;

public class ParentFolderInput
{
    [Display("Folder path")]
    [FileDataSource(typeof(FolderPickerDataSourceHandler))]
    public string? ParentFolderLowerPath { get; set; }
}