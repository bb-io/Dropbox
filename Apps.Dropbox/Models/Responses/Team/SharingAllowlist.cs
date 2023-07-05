namespace Apps.Dropbox.Models.Responses.Team;

public class SharingAllowlist
{
    public List<string> Emails { get; init; } = new();
    public List<string> Domains { get; init; } = new();
}