namespace Apps.Dropbox.Utils;

public class FileNameHelper
{
    public static string EnsureCorrectFilename(string currentFilePath, string? targetFilename)
    {
        var originalFilename = Path.GetFileName(currentFilePath);
        var originalExtension = Path.GetExtension(originalFilename);

        var filename = targetFilename;
        if (string.IsNullOrEmpty(filename))
        {
            filename = originalFilename; 
        }
        else if (!Path.HasExtension(filename))
        {
            filename += originalExtension;
        }
        
        return filename;
    }
}