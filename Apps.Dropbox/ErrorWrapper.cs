using Apps.Dropbox.Constants;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace Apps.Dropbox;

public static class ErrorWrapper
{
    private static readonly Dictionary<string, string> DropboxErrorMessages = new()
        {
            { "cant_copy_shared_folder", "Shared folders can't be copied." },
            { "cant_nest_shared_folder", "Your move operation would result in nested shared folders. This is not allowed." },
            { "cant_move_folder_into_itself", "You cannot move a folder into itself." },
            { "too_many_files", "The operation would involve more than 10,000 files and folders." },
            { "duplicated_or_nested_paths", "There are duplicated/nested paths among Current or Destination path." },
            { "cant_transfer_ownership", "Your move operation would result in an ownership transfer. Check the ownership permission." },
            { "insufficient_quota", "The current user does not have enough space to move or copy the files." },
            { "internal_error", "Something went wrong on Dropbox's end. Please verify the action succeeded, and if not, try again." },
            { "to/conflict/file", "A conflict occurred with the file in the destination folder. Please resolve the conflict and try again." },
            { "path/not_found/..", "Nothing was found at the given path, please change the input for path" },
            { "path/not_found/", "Nothing was found at the given path, please change the input for path" },
            { "path_lookup/not_found/", "No file/folder was found at the given path, please change the input for path" },

        };
    private static readonly Dictionary<string, string> GeneralErrorMessages = ErrorMessages.ErrorMessagesDictionary;
    public static async Task<T> WrapError<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (global::Dropbox.Api.ApiException<RelocationError> apiEx)
        {
            string errorMessage = ExtractDropboxErrorMessage(apiEx);

            foreach (var errorKeyValue in DropboxErrorMessages)
            {
                if (errorMessage.Contains(errorKeyValue.Key, StringComparison.OrdinalIgnoreCase))
                {
                    throw new PluginMisconfigurationException(errorKeyValue.Value);
                }
            }
            foreach (var errorKeyValue in GeneralErrorMessages)
            {
                if (errorMessage.Contains(errorKeyValue.Key, StringComparison.OrdinalIgnoreCase))
                {
                    throw new PluginMisconfigurationException(errorKeyValue.Value);
                }
            }

            throw new PluginApplicationException($"An unexpected Dropbox error occurred: {errorMessage}");
        }
        catch (Exception e)
        {
            string generalErrorMessage = e.Message;
            foreach (var errorKeyValue in DropboxErrorMessages)
            {
                if (generalErrorMessage.Contains(errorKeyValue.Key, StringComparison.OrdinalIgnoreCase))
                {
                    throw new PluginMisconfigurationException(errorKeyValue.Value);
                }
            }

            foreach (var errorKeyValue in GeneralErrorMessages)
            {
                if (generalErrorMessage.Contains(errorKeyValue.Key, StringComparison.OrdinalIgnoreCase))
                {
                    throw new PluginMisconfigurationException(errorKeyValue.Value);
                }
            }
            throw new PluginApplicationException($"An unexpected error occurred: {generalErrorMessage}");
        }
    }

    private static string ExtractDropboxErrorMessage(global::Dropbox.Api.ApiException<RelocationError> apiEx)
    {
        return apiEx.Message;
    }
}
