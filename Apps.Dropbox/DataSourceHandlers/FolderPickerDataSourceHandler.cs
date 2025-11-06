using Apps.Dropbox.Invocables;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using Dropbox.Api.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.Dropbox.DataSourceHandlers
{
    public class FolderPickerDataSourceHandler(InvocationContext invocationContext) : DropboxInvocable(invocationContext), IAsyncFileDataSourceItemHandler
    {
        private const string RootFolderDisplayName = "My files";
        private const string RootId = "root";

        public async Task<IEnumerable<FileDataItem>> GetFolderContentAsync(
       FolderContentDataSourceContext context,
       CancellationToken cancellationToken)
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
                if (entries.Count >= 2000) break;
            }

            return entries
                .Where(e => e.IsFolder)
                .Select(e => new Folder
                {
                    Id = e.AsFolder.Id, 
                    DisplayName = e.Name,
                    Date = null,
                    IsSelectable = true
                })
                .Cast<FileDataItem>()
                .ToList();
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

            try
            {
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

                var take = md.IsFolder ? segments.Length : segments.Length - 1;

                for (int i = 0; i < take; i++)
                {
                    var display = segments[i];
                    var cumulativePath = "/" + string.Join("/", segments.Take(i + 1));

                    var folderMd = await client.Files.GetMetadataAsync(new GetMetadataArg(cumulativePath));
                    var folderId = folderMd.IsFolder ? folderMd.AsFolder.Id : cumulativePath;

                    items.Add(new FolderPathItem
                    {
                        DisplayName = display,
                        Id = folderId
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
            catch
            {
                return new List<FolderPathItem>
            {
                new() { DisplayName = RootFolderDisplayName, Id = RootId }
            };
            }
        }
    }
}
