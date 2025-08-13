using Tests.Dropbox.Base;

namespace Tests.Dropbox
{
    [TestClass]
    public class StorageTests : TestBase
    {
        [TestMethod]
        public async Task DownloadAllFiles_ShouldReturnFiles()
        {
            var action = new Apps.Dropbox.Actions.StorageActions(InvocationContext, FileManager);

            var response = await action.DownloadAllFiles(new Apps.Dropbox.Models.Requests.DownloadFolderRequest
            {
                FolderPath = "/Input",
                SubfolderScope = "recursive"
            });

            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            Console.WriteLine(jsonResponse);
            Assert.IsNotNull(response);
        }
    }
}
