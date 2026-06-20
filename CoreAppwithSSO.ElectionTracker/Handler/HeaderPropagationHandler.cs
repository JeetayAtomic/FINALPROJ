namespace CoreAppwithSSO.ElectionTracker.Handler
{
    public class HeaderPropagationHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var context = httpContextAccessor.HttpContext;

            if (context != null &&
                context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                request.Headers.TryAddWithoutValidation("Authorization", authHeader.ToString());
            }
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
