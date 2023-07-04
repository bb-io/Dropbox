namespace Apps.Dropbox.Models.Responses.Team.List;

public class ListGroupsResponse
{
    public ListGroupsResponse(List<GroupResponse> groups)
    {
        Groups = groups;
    }

    public List<GroupResponse> Groups { get; init; }
}