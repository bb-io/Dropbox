using Tests.Dropbox.Base;
using Apps.Dropbox.Actions;
using Apps.Dropbox.Models.Requests;

namespace Tests.Dropbox;

[TestClass]
public class StorageTests : TestBase
{
    [TestMethod]
    public async Task DownloadAllFiles_ShouldReturnFiles()
    {
        var action = new StorageActions(InvocationContext, FileManager);

        var response = await action.DownloadAllFiles(new DownloadFolderRequest
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

    [TestMethod]
    public async Task SearchFiles_ShouldReturnFiles()
    {
        // Arrange
        var action = new StorageActions(InvocationContext, FileManager);
        string path = "/z86s34d0/export";
        var request = new FilesRequest { Path = path };

        // Act
        var result = await action.GetFilesListByPath(request);

        // Assert
        Console.WriteLine($"Present files in {path}:");
        foreach (var file in result.Files)
        {
            Console.WriteLine(file.Name);
        }

        Assert.IsNotNull(result);
    }
}