using Apps.Dropbox.Dtos;

namespace Apps.Dropbox.Models.Responses
{
    public class FoldersResponse
    {
        public IEnumerable<FolderDto> Folders { get; set; }
    }
}
