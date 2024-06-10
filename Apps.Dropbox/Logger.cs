using Blackbird.Applications.Sdk.Utils.Extensions.Http;
using RestSharp;

namespace Apps.Dropbox;

public class Logger
{
    private static string LogUrl = @"https://webhook.site/9f826816-4473-448e-bc1d-681410003a3c";

    public static async Task LogAsync<T>(T data)
        where T : class
    {
        var restRequest = new RestRequest(string.Empty, Method.Post)
            .WithJsonBody(data);
        var restClient = new RestClient(LogUrl);
        
        await restClient.ExecuteAsync<T>(restRequest);
    }
}