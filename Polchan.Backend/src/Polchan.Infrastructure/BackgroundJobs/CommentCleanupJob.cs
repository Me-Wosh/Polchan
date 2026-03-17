using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polchan.Application.Interfaces;
using Polchan.Core.Interfaces;

namespace Polchan.Infrastructure.BackgroundJobs;

public class CommentCleanupJob(
    IPolchanDbContext dbContext,
    ILogger<CommentCleanupJob> logger,
    IBackgroundJobClient backgroundJobClient
) : ICommentCleanupJob
{
    private const int BatchSize = 1000;

    public async Task CleanupCommentAsync(Guid id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Initiating cleanup of comment {Id}", id);

        var commentExists = await dbContext
            .Comments
            .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
            .AnyAsync(c => c.Id == id, cancellationToken);

        if (!commentExists)
        {
            logger.LogWarning("Comment with Id {Id} not found, aborting cleanup", id);
            return;
        }

        var reactionsExist = await dbContext
            .Reactions
            .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
            .AnyAsync(r => r.CommentId == id, cancellationToken);

        if (reactionsExist)
        {
            var deletedRowsCount = await dbContext
                .Reactions
                .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
                .Where(r => r.CommentId == id)
                .Take(BatchSize)
                .ExecuteDeleteAsync(cancellationToken);

            logger.LogInformation(
                "Deleted {DeletedRowsCount} comment reactions. Scheduling next cleanup background job for comment {Id}",
                deletedRowsCount,
                id
            );

            backgroundJobClient.Enqueue<ICommentCleanupJob>(job => job.CleanupCommentAsync(id, CancellationToken.None));
            return;
        }

        logger.LogInformation("No reactions to delete for comment {Id}, deleting the comment itself", id);

        var deletedRows = await dbContext
            .Comments
            .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
            .Where(c => c.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedRows == 1)
            logger.LogInformation("Comment {Id} deleted successfully", id);
        else
            logger.LogWarning("No comment was deleted");
    }
}
