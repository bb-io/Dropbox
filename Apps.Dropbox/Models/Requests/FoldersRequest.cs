using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.Dropbox.Models.Requests
{
    public class FoldersRequest
    {
        [FileDataSource(typeof(FolderPickerDataSourceHandler))]
        public string Path { get; set; }
    }
}
