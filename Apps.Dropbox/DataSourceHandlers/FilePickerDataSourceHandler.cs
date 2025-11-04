using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using Dropbox.Api.Files;
using File = Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems.File;

namespace Apps.Dropbox.DataSourceHandlers
{
    public class FilePickerDataSourceHandler(InvocationContext invocationContext) : BaseInvocable(invocationContext), IAsyncFileDataSourceItemHandler
    {
        private const string RootFolderDisplayName = "Dropbox";
        private const string RootId = "root";

        public async Task<IEnumerable<FileDataItem>> GetFolderContentAsync(FolderContentDataSourceContext context,CancellationToken cancellationToken)
        {
            var folderIdOrPath = string.IsNullOrEmpty(context?.FolderId) || context.FolderId == RootId
                ? string.Empty
                : context.FolderId;

            using var client = DropboxClientFactory.CreateDropboxClient(InvocationContext.AuthenticationCredentialsProviders);

            var entries = new List<Metadata>();
            var list = await client.Files.ListFolderAsync(new ListFolderArg(folderIdOrPath, recursive: false, limit: 200));

            entries.AddRange(list.Entries);

            while (list.HasMore)
            {
                list = await client.Files.ListFolderContinueAsync(list.Cursor);
                entries.AddRange(list.Entries);
                if (entries.Count >= 1000) break;
            }

            var result = new List<FileDataItem>();
            foreach (var e in entries)
            {
                if (e.IsFolder)
                {
                    var f = e.AsFolder;
                    result.Add(new Folder
                    {
                        Id = f.Id,
                        DisplayName = e.Name,
                        Date = null,
                        IsSelectable = false
                    });
                }
                else if (e.IsFile)
                {
                    var f = e.AsFile;
                    result.Add(new File
                    {
                        Id = f.Id,
                        DisplayName = e.Name,
                        Date = f.ServerModified,
                        Size = f.Size <= long.MaxValue ? (long)f.Size : long.MaxValue,
                        IsSelectable = true 
                    });
                }
            }

            return result;
        }

        public async Task<IEnumerable<FolderPathItem>> GetFolderPathAsync(
            FolderPathDataSourceContext context,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(context?.FileDataItemId))
            {
                return new List<FolderPathItem>
                {
                    new() { DisplayName = RootFolderDisplayName, Id = RootId }
                };
            }

            using var client = DropboxClientFactory.CreateDropboxClient(InvocationContext.AuthenticationCredentialsProviders);

            var md = await client.Files.GetMetadataAsync(new GetMetadataArg(context.FileDataItemId));

            var path = md.PathLower;

            if (string.IsNullOrEmpty(path) || path == "/")
            {
                return new List<FolderPathItem>
                {
                    new() { DisplayName = RootFolderDisplayName, Id = RootId }
                };
            }

            var segments = path.Trim('/').Split('/');
            var items = new List<FolderPathItem>();

            for (int i = 0; i < segments.Length - (md.IsFile ? 1 : 0); i++)
            {
                var display = segments[i];
                var cumulative = "/" + string.Join("/", segments.Take(i + 1));

                items.Add(new FolderPathItem
                {
                    DisplayName = display,
                    Id = cumulative
                });
            }

            if (items.Any())
            {
                items[0].DisplayName = RootFolderDisplayName;
                items[0].Id = RootId;
            }
            else
            {
                items.Insert(0, new FolderPathItem { DisplayName = RootFolderDisplayName, Id = RootId });
            }

            return items;
        }
    }
}
