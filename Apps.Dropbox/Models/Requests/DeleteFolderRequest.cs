using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Dropbox.Models.Requests;

public class DeleteFolderRequest
{
    [DataSource(typeof(FolderDataSourceHandler))]
    public string FolderPath { get; set; }
}