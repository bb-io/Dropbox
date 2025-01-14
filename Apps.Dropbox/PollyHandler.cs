using Polly;

namespace Apps.Dropbox
{
    public class PollyHandler : DelegatingHandler
    {
        private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

        public PollyHandler(IAsyncPolicy<HttpResponseMessage> retryPolicy)
        {
            _retryPolicy = retryPolicy;
        }

        protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
        {
            return _retryPolicy.ExecuteAsync(
                () => base.SendAsync(request, cancellationToken)
            );
        }
    }
}
