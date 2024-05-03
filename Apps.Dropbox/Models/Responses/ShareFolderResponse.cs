using Blackbird.Applications.Sdk.Common;
using Dropbox.Api.Sharing;

namespace Apps.Dropbox.Models.Responses;

public class ShareFolderResponse
{
    [Display("Is complete")]
    public bool IsComplete { get; set; }

    [Display("Is async job")]
    public bool IsAsyncJob { get; set; }

    [Display("Async job id")]
    public string AsyncJobId { get; set; }

    public string Name { get; set; }

    [Display("Preview URL")]
    public string PreviewUrl { get; set; }
    
    [Display("Shared folder id")]
    public string SharedFolderId { get; set; }
}