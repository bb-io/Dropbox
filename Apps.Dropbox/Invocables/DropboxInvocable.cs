using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Dropbox.Api;

namespace Apps.Dropbox.Invocables;
public class DropboxInvocable : BaseInvocable
{
    protected AuthenticationCredentialsProvider[] Creds => InvocationContext.AuthenticationCredentialsProviders.ToArray();
    public DropboxClient Client { get; set; }

    public DropboxInvocable(InvocationContext invocationContext) : base(invocationContext)
    {
        Client = DropboxClientFactory.CreateDropboxClient(Creds);
    }
}
