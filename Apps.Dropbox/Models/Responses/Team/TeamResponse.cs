using Blackbird.Applications.Sdk.Common;

namespace Apps.Dropbox.Models.Responses.Team;

public class TeamResponse
{
    public string Name { get; init; }
    [Display("Team id")] public string TeamId { get; init; }
    [Display("Number of licensed users")] public uint NumLicensedUsers { get; init; }
    [Display("Number of provisioned users")] public uint NumProvisionedUsers { get; init; }
    [Display("Number of used licenses")] public uint NumUsedLicenses { get; init; }
}