using Apps.Dropbox.Constants;

namespace Apps.Dropbox;

public static class ErrorWrapper
{
    public static async Task<T> WrapError<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception e)
        {
            foreach (var errorKeyValue in ErrorMessages.ErrorMessagesDictionary)
            {
                if (e.Message.Contains(errorKeyValue.Key))
                {
                    throw new InvalidOperationException(errorKeyValue.Value);
                }
            }
            
            throw;        
        }
    }
}