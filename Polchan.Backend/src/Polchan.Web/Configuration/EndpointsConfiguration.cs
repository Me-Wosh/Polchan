using Polchan.Web.Endpoints;
using Polchan.Web.Endpoints.Filters;

namespace Polchan.Web.Configuration;

public static class EndpointsConfiguration
{
    public static void MapEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");
        api.AddEndpointFilter<ArdalisResultMapper>();
        api.MapAuthEndpoints();
        api.MapThreadEndpoints();
    }
}
