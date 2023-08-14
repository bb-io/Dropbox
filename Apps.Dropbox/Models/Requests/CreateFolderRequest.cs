using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Dropbox.Models.Requests
{
    public class CreateFolderRequest
    {
        [DataSource(typeof(FolderDataSourceHandler))]
        public string ParentFolderPath { get; set; }

        public string FolderName { get; set; }
    }
}
