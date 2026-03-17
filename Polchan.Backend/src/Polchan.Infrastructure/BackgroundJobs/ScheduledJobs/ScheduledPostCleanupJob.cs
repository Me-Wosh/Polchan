using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polchan.Application.Interfaces;
using Polchan.Core.Interfaces;

namespace Polchan.Infrastructure.BackgroundJobs.ScheduledJobs;

public class ScheduledPostCleanupJob(
    ILogger<ScheduledPostCleanupJob> logger,
    IPolchanDbContext dbContext,
    IBackgroundJobClient backgroundJobClient
)
{
    public async Task CleanupPostAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Running scheduled cleanup of soft-deleted posts");

        var postId = await dbContext
            .Posts
            .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
            .Where(c => c.SoftDeleted)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (postId == Guid.Empty)
        {
            logger.LogInformation("No soft-deleted posts found, skipping cleanup");
            return;
        }

        logger.LogInformation("Starting scheduled cleanup of post {Id}", postId);
        backgroundJobClient.Enqueue<IPostCleanupJob>(job => job.CleanupPostAsync(postId, CancellationToken.None));
    }
}
