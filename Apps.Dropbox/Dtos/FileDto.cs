using Blackbird.Applications.Sdk.Common;
using Dropbox.Api.Files;

namespace Apps.Dropbox.Dtos;

public class FileDto : ItemDto
{
    public FileDto(FileMetadata file) : base(file)
    {
        SizeInBytes = file.Size;
    }

    [Display("Size in bytes")]
    public ulong SizeInBytes { get; set; }
}