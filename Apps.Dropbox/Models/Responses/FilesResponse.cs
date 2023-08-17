using Apps.Dropbox.Dtos;

namespace Apps.Dropbox.Models.Responses
{
    public class FilesResponse
    {
        public IEnumerable<FileDto> Files { get; set; }
    }
}
