using Blackbird.Applications.Sdk.Common;
using Dropbox.Api.Files;

namespace Apps.Dropbox.Dtos;

public class FolderDto
{
    public FolderDto(FolderMetadata folder)
    {
        FolderPath = folder.PathDisplay;
    }

    [Display("Folder path")]
    public string FolderPath { get; set; }
}