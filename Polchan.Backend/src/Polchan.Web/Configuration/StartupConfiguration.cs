using Polchan.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Polchan.Web.Configuration;

public static class StartupConfiguration
{
    public static void ConfigureStartup(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PolchanDbContext>();
        db.Database.Migrate();
    }
}
