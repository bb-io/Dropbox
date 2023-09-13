using Dropbox.Api.Files;

namespace Apps.Dropbox.Dtos;

public class DeletedItemDto : ItemDto
{
    public DeletedItemDto(DeletedMetadata item) : base(item) { }
}