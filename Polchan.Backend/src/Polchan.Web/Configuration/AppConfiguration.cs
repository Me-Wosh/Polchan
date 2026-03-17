using Hangfire;
using Polchan.Infrastructure.BackgroundJobs.ScheduledJobs;
using Serilog;

namespace Polchan.Web.Configuration;

public static class AppConfiguration
{
    public static void ConfigureApp(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseHangfireDashboard();
        }

        if (app.Environment.IsProduction())
            app.UseExceptionHandler();
        
        app.UseSerilogRequestLogging();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapEndpoints();
        app.UseScheduledJobs();
    }

    private static void UseScheduledJobs(this WebApplication app)
    {
        var jobManager = app.Services.GetRequiredService<IRecurringJobManager>();
        
        jobManager.AddOrUpdate<ScheduledCommentCleanupJob>(
            "ScheduledCommentCleanupJob",
            job => job.CleanupCommentAsync(CancellationToken.None),
            app.Configuration.GetRequiredSection("BackgroundJobs:ScheduledJobs:CommentCleanup:CronExpression").Value
        );

        jobManager.AddOrUpdate<ScheduledPostCleanupJob>(
            "ScheduledPostCleanupJob",
            job => job.CleanupPostAsync(CancellationToken.None),
            app.Configuration.GetRequiredSection("BackgroundJobs:ScheduledJobs:PostCleanup:CronExpression").Value
        );

        jobManager.AddOrUpdate<ScheduledThreadCleanupJob>(
            "ScheduledThreadCleanupJob",
            job => job.CleanupThreadAsync(CancellationToken.None),
            app.Configuration.GetRequiredSection("BackgroundJobs:ScheduledJobs:ThreadCleanup:CronExpression").Value
        );
    }
}
