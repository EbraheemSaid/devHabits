using DevHabit.Api.DTOs.Common;

namespace DevHabit.Api.Services;

public sealed class LinkService(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
{
    public LinkDto Create(
        string endpointName,
        string rel,
        string method,
        object? values = null,
        string? controller = null)
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HTTP context is not available.");

        var href = linkGenerator.GetUriByAction(
            httpContext,
            endpointName,
            controller,
            values);

        if (href is null)
        {
            throw new Exception($"Could not generate link for endpoint '{endpointName}'.");
        }

        return new LinkDto
        {
            Href = href,
            Rel = rel,
            Method = method
        };
    }

}
