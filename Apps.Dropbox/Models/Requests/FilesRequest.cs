using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.Dropbox.Models.Requests;

public class FilesRequest
{
    [FileDataSource(typeof(FolderPickerDataSourceHandler))]
    public string Path { get; set; }

    [Display("Modified after")]
    public DateTime? ModifiedAfter { get; set; }

    [Display("Modified before")]
    public DateTime? ModifiedBefore { get; set; }
}
