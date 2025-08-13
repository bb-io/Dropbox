using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Dropbox.DataSourceHandlers.Enum
{
    public class SubfolderDataHandler : IStaticDataSourceItemHandler
    {
        public IEnumerable<DataSourceItem> GetData() =>
        [
            new DataSourceItem("none", "Only this folder"),
            new DataSourceItem("immediate", "This folder + immediate subfolders"),
            new DataSourceItem("recursive", "This folder + all nested subfolders")
        ];
    }
}