using Serilog;

namespace Polchan.Web.Configuration;

public static class AppConfiguration
{
    public static void ConfigureApp(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
            app.MapOpenApi();

        if (app.Environment.IsProduction())
            app.UseExceptionHandler();
        
        app.UseSerilogRequestLogging();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapEndpoints();
    }
}
