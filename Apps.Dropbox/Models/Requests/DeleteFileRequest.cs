using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.Dropbox.Models.Requests;

public class DeleteFileRequest
{
    [Display("File path")]
    [FileDataSource(typeof(FilePickerDataSourceHandler))]
    public string FilePath { get; set; }
}