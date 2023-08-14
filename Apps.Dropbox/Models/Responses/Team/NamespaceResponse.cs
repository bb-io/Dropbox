using Blackbird.Applications.Sdk.Common;
using Dropbox.Api.Team;

namespace Apps.Dropbox.Models.Responses.Team;

public class NamespaceResponse
{
    public NamespaceResponse(NamespaceMetadata namespaceMetadata)
    {
        Id = namespaceMetadata.NamespaceId;
        Name = namespaceMetadata.Name;
        TeamMemberId = namespaceMetadata.TeamMemberId;
    }

    [Display("Namespace ID")]
    public string Id { get; set; }
    
    [Display("Namespace name")]
    public string Name { get; set; }
    
    [Display("Team member ID")]
    public string TeamMemberId { get; set; }
}