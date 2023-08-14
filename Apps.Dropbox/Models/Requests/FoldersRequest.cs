using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Dropbox.Models.Requests
{
    public class FoldersRequest
    {
        [DataSource(typeof(FolderDataSourceHandler))]
        public string Path { get; set; }
    }
}
