using Apps.Dropbox.Constants;

namespace Apps.Dropbox;

public class ErrorWrapper
{
    public static async Task<T> WrapError<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception e)
        {
            if(e.Message.Contains(ErrorMessages.TokenNotAssociatedWithTeam))
            {
                throw new InvalidOperationException("This token is not associated with a team");
            }
            
            throw;        
        }
    }
}