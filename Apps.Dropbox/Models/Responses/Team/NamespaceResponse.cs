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

    public string Id { get; set; }
    public string Name { get; set; }
    public string TeamMemberId { get; set; }
}