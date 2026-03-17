using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polchan.Application.Interfaces;
using Polchan.Core.Interfaces;

namespace Polchan.Infrastructure.BackgroundJobs.ScheduledJobs;

public class ScheduledCommentCleanupJob(
    ILogger<ScheduledCommentCleanupJob> logger,
    IPolchanDbContext dbContext,
    IBackgroundJobClient backgroundJobClient
)
{
    public async Task CleanupCommentAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Running scheduled cleanup of soft-deleted comments");

        var commentId = await dbContext
            .Comments
            .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
            .Where(c => c.SoftDeleted)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (commentId == Guid.Empty)
        {
            logger.LogInformation("No soft-deleted comments found, skipping cleanup");
            return;
        }

        logger.LogInformation("Starting scheduled cleanup of comment {Id}", commentId);
        backgroundJobClient.Enqueue<ICommentCleanupJob>(job => job.CleanupCommentAsync(commentId, CancellationToken.None));
    }
}
