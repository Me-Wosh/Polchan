using Polchan.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Polchan.Web.Configuration;

public static class ServicesConfiguration
{
    public static void AddServices(this IServiceCollection services, WebApplicationBuilder builder)
    {
        services.AddOpenApi();
        
        services.AddDbContext<PolchanDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("PolchanDB"));
        });
    }
}
