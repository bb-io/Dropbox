using Blackbird.Applications.Sdk.Common;
using Dropbox.Api.Files;

namespace Apps.Dropbox.Dtos;

public class FileDto
{
    public FileDto(FileMetadata file)
    {
        FilePath = file.PathDisplay;
        SizeInBytes = file.Size;
    }

    [Display("File path")]
    public string FilePath { get; set; }
    
    [Display("Size in bytes")]
    public ulong SizeInBytes { get; set; }
}