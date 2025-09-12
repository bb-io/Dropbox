using Tests.Dropbox.Base;
using Apps.Dropbox.Webhooks;
using Apps.Dropbox.Webhooks.Inputs;
using Apps.Dropbox.Webhooks.Polling.Memory;
using Blackbird.Applications.Sdk.Common.Polling;

namespace Tests.Dropbox;

[TestClass]
public class PollingTests : TestBase
{
    [TestMethod]
    public async Task OnFilesAddedOrUpdated_IsSuccess()
    {
        // Arrange
        var action = new PollingList(InvocationContext);
        string cursorString = "AATR40MfMqwNhW6OXFKyn0J3X1DgEN6tXMdXdFAfhvxKU0Ar5tj3xVirRzJOrm5rXX4bP0ehw3LfVoqtwKYW4myRyGagJDpl1B73BSjPlWgVyFZcSrhuhcv4VrriCrn8BCGiEAOjsdqLmUZinY1M6J7KsfFE78FWKXZVfk1pbyAgUZLipwyjxROTrbcXHhOSwtSVwpusdAG3V_SyEe9tLbAb9ujUrdNg3QbibKnnrWgNaA";

        var cursor = new CursorMemory { Cursor = cursorString };
        var request = new PollingEventRequest<CursorMemory> { Memory = cursor };
        var folder = new ParentFolderInput { ParentFolderLowerPath = "/z86s34d0/export" };

        // Act
        var result = await action.OnFilesAddedOrUpdated(request, folder);

        // Assert
        Console.WriteLine("Changed files:");
        foreach (var file in result.Result.Files)
        {
            Console.WriteLine(file.Name);
        }

        Assert.IsNotNull(result);
    }
}
