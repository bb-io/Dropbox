using Apps.Dropbox.Invocables;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using Dropbox.Api;
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
        private const string RootPath = "/";
        private const string RootId = "root";

        public async Task<IEnumerable<FileDataItem>> GetFolderContentAsync(
       FolderContentDataSourceContext context,
       CancellationToken cancellationToken)
        {
            using var client = DropboxClientFactory.CreateDropboxClient(InvocationContext.AuthenticationCredentialsProviders);

            var folderPath = await EnsurePathAsync(client, context?.FolderId);

            var entries = new List<Metadata>();
            var list = await client.Files.ListFolderAsync(new ListFolderArg(
                path: folderPath == RootPath ? string.Empty : folderPath,
                recursive: false,
                limit: 200));

            entries.AddRange(list.Entries);
            while (list.HasMore)
            {
                list = await client.Files.ListFolderContinueAsync(list.Cursor);
                entries.AddRange(list.Entries);
                if (entries.Count >= 2000) break;
            }

            var result = new List<FileDataItem>();
            foreach (var e in entries.Where(x => x.IsFolder))
            {
                var path = e.PathLower;
                if (string.IsNullOrEmpty(path))
                {
                    var md = await client.Files.GetMetadataAsync(new GetMetadataArg(e.AsFolder.Id));
                    path = md.PathLower;
                }

                result.Add(new Folder
                {
                    Id = NormalizePath(path),
                    DisplayName = e.Name,
                    Date = null,
                    IsSelectable = true
                });
            }

            return result;
        }

        public async Task<IEnumerable<FolderPathItem>> GetFolderPathAsync(
            FolderPathDataSourceContext context,
            CancellationToken cancellationToken)
        {
            using var client = DropboxClientFactory.CreateDropboxClient(InvocationContext.AuthenticationCredentialsProviders);

            if (string.IsNullOrEmpty(context?.FileDataItemId))
            {
                return new List<FolderPathItem> { new() { DisplayName = RootFolderDisplayName, Id = RootId } };
            }

            var path = await EnsurePathAsync(client, context.FileDataItemId);

            if (string.IsNullOrEmpty(path) || path == RootPath)
            {
                return new List<FolderPathItem> { new() { DisplayName = RootFolderDisplayName, Id = RootId } };
            }

            var segments = path.Trim('/').Split('/');
            var items = new List<FolderPathItem>();

            try
            {
                var md = await client.Files.GetMetadataAsync(new GetMetadataArg(path));
                if (md.IsFile && segments.Length > 0)
                    segments = segments.Take(segments.Length - 1).ToArray();
            }
            catch
            {
            }

            for (int i = 0; i < segments.Length; i++)
            {
                var display = segments[i];
                var cumulativePath = "/" + string.Join("/", segments.Take(i + 1));

                items.Add(new FolderPathItem
                {
                    DisplayName = display,
                    Id = NormalizePath(cumulativePath)
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

        private static async Task<string> EnsurePathAsync(DropboxClient client, string? idOrPath)
        {
            if (string.IsNullOrWhiteSpace(idOrPath) || idOrPath == RootId)
                return RootPath;

            if (idOrPath.StartsWith("/"))
                return NormalizePath(idOrPath);

            var md = await client.Files.GetMetadataAsync(new GetMetadataArg(idOrPath));
            var path = md.PathLower;

            return string.IsNullOrEmpty(path) ? RootPath : NormalizePath(path);
        }

        private static string NormalizePath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return RootPath;
            var p = path.Trim();
            if (!p.StartsWith("/")) p = "/" + p;
            return p == "//" ? RootPath : p;
        }
    }
}
