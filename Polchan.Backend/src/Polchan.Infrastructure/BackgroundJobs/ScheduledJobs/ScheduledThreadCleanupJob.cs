using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polchan.Application.Interfaces;
using Polchan.Core.Interfaces;

namespace Polchan.Infrastructure.BackgroundJobs.ScheduledJobs;

public class ScheduledThreadCleanupJob(
    ILogger<ScheduledThreadCleanupJob> logger,
    IPolchanDbContext dbContext,
    IBackgroundJobClient backgroundJobClient
)
{
    public async Task CleanupThreadAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Running scheduled cleanup of soft-deleted threads");

        var threadId = await dbContext
            .Threads
            .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
            .Where(c => c.SoftDeleted)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (threadId == Guid.Empty)
        {
            logger.LogInformation("No soft-deleted threads found, skipping cleanup");
            return;
        }

        logger.LogInformation("Starting scheduled cleanup of thread {Id}", threadId);
        backgroundJobClient.Enqueue<IThreadCleanupJob>(job => job.CleanupThreadAsync(threadId, CancellationToken.None));
    }
}
