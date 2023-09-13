using Dropbox.Api.Files;

namespace Apps.Dropbox.Dtos;

public class FolderDto : ItemDto
{
    public FolderDto(FolderMetadata folder) : base(folder) { }
}