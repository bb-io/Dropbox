using Apps.Dropbox.DataSourceHandlers;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using Tests.Dropbox.Base;

namespace Tests.Dropbox
{
    [TestClass]
    public class DataHandlerTests : TestBase
    {
        [TestMethod]
        public async Task FilePickerDataSourceHandler_GetFolderContentAsync_ShouldReturnItems()
        {
            var handler = new FilePickerDataSourceHandler(InvocationContext);
            var result = await handler.GetFolderContentAsync(new FolderContentDataSourceContext
            {
                FolderId = string.Empty
            }, CancellationToken.None);
            var itemList = result.ToList();
            foreach (var item in itemList)
            {
                Console.WriteLine($"Item: {item.DisplayName}, Id: {item.Id}, Type: {(item is Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems.Folder ? "Folder" : "File")}");
            }
            Assert.IsNotNull(result);
            Assert.IsTrue(itemList.Count > 0, "The folder should contain items.");
        }
    }
}
