using Blackbird.Applications.Sdk.Common;

namespace Apps.Dropbox
{
    public class DropboxApplication : IApplication
    {
        public string Name {
            get => "Dropbox Application";
            set { }
        }

        public T GetInstance<T>()
        {
            throw new NotImplementedException();
        }
    }
}
