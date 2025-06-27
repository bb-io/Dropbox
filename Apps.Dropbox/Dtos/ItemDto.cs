using Blackbird.Applications.Sdk.Common;
using Dropbox.Api.Files;

namespace Apps.Dropbox.Dtos;

public class ItemDto
{
    public ItemDto(Metadata item)
    {
        Name = item.Name;
        FileId = item.PathDisplay;
    }
    
    public string Name { get; set; }
    
    [Display("File path")]
    public string FileId { get; set; }
}