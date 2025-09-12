using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Dropbox.Models.Requests;

public class FilesRequest
{
    [DataSource(typeof(FolderDataSourceHandler))]
    public string Path { get; set; }

    [Display("Modified after")]
    public DateTime? ModifiedAfter { get; set; }

    [Display("Modified before")]
    public DateTime? ModifiedBefore { get; set; }
}
