namespace Apps.Dropbox.Constants;

public class ErrorMessages
{
    public const string TokenNotAssociatedWithTeam = "This token is not associated with a team";
    public const string RootFolderUnsupported = "The root folder is unsupported";
    
    public static Dictionary<string, string> ErrorMessagesDictionary = new()
    {
        { TokenNotAssociatedWithTeam, "This token is not associated with a team" },
        { RootFolderUnsupported, "The root folder is unsupported" }
    }; 
}