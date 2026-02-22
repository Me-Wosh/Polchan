using System.Reflection;
using Polchan.Web.Endpoints;
using Polchan.Web.Endpoints.Filters;

namespace Polchan.Web.Configuration;

public static class EndpointsConfiguration
{
    public static void MapEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");
        api.AddEndpointFilter<ArdalisResultMapper>();
        api.DisableAntiforgery();
        
        var endpointGroups = Assembly.GetExecutingAssembly()
            .DefinedTypes
            .Where(t => t.IsAssignableTo(typeof(IEndpointGroup)) && !t.IsInterface && !t.IsAbstract);

        foreach (var group in endpointGroups)
        {
            var instance = Activator.CreateInstance(group) as IEndpointGroup;
            instance?.MapEndpoints(api);
        }
    }
}
