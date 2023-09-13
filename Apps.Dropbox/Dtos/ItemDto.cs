using Blackbird.Applications.Sdk.Common;
using Dropbox.Api.Files;

namespace Apps.Dropbox.Dtos;

public class ItemDto
{
    public ItemDto(Metadata item)
    {
        Name = item.Name;
        FullPath = item.PathDisplay;
    }
    
    public string Name { get; set; }
    
    [Display("Full path")]
    public string FullPath { get; set; }
}