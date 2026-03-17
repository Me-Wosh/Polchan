using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polchan.Application.Interfaces;
using Polchan.Core.Interfaces;

namespace Polchan.Infrastructure.BackgroundJobs;

public class ThreadCleanupJob(
    ILogger<ThreadCleanupJob> logger,
    IPolchanDbContext dbContext,
    IBackgroundJobClient backgroundJobClient
) : IThreadCleanupJob
{
    private const int BatchSize = 1000;

    public async Task CleanupThreadAsync(Guid id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Initiating cleanup of thread {Id}", id);

        var threadExists = await dbContext
            .Threads
            .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
            .AnyAsync(t => t.Id == id, cancellationToken);

        if (!threadExists)
        {
            logger.LogWarning("Thread with Id {Id} not found", id);
            return;
        }

        var postReactionsExist = await dbContext
            .Reactions
            .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
            .AnyAsync(r => r.Post != null && r.Post.ThreadId == id, cancellationToken);

        if (postReactionsExist)
        {
            var deletedRowsCount = await dbContext
                .Reactions
                .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
                .Where(r => r.Post != null && r.Post.ThreadId == id)
                .Take(BatchSize)
                .ExecuteDeleteAsync(cancellationToken);

            logger.LogInformation(
                "Deleted {DeletedRowsCount} post reactions. Scheduling next cleanup background job for thread {Id}",
                deletedRowsCount,
                id
            );

            backgroundJobClient.Enqueue<IThreadCleanupJob>(job => job.CleanupThreadAsync(id, CancellationToken.None));
            return;
        }

        logger.LogInformation("No post reactions to delete, initiating comment reactions cleanup");

        var commentReactionsExist = await dbContext
            .Reactions
            .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
            .AnyAsync(r => r.Comment != null && r.Comment.Post.ThreadId == id, cancellationToken);

        if (commentReactionsExist)
        {
            var deletedRowsCount = await dbContext
                .Reactions
                .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
                .Where(r => r.Comment != null && r.Comment.Post.ThreadId == id)
                .Take(BatchSize)
                .ExecuteDeleteAsync(cancellationToken);

            logger.LogInformation(
                "Deleted {DeletedRowsCount} comment reactions. Scheduling next cleanup background job for thread {Id}",
                deletedRowsCount,
                id
            );

            backgroundJobClient.Enqueue<IThreadCleanupJob>(job => job.CleanupThreadAsync(id, CancellationToken.None));
            return;
        }

        logger.LogInformation("No reactions to delete for comments, deleting the comments");

        var commentsExist = await dbContext
            .Comments
            .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
            .AnyAsync(c => c.Post.ThreadId == id, cancellationToken);

        if (commentsExist)
        {
            var deletedRowsCount = await dbContext
                .Comments
                .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
                .Where(c => c.Post.ThreadId == id)
                .Take(BatchSize)
                .ExecuteDeleteAsync(cancellationToken);

            logger.LogInformation(
                "Deleted {DeletedRowsCount} comments. Scheduling next cleanup background job for thread {Id}",
                deletedRowsCount,
                id
            );

            backgroundJobClient.Enqueue<IThreadCleanupJob>(job => job.CleanupThreadAsync(id, CancellationToken.None));
            return;
        }

        logger.LogInformation("No comments to delete, deleting the posts");

        var postsExist = await dbContext
            .Posts
            .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
            .AnyAsync(p => p.ThreadId == id, cancellationToken);

        if (postsExist)
        {
            var deletedRowsCount = await dbContext
                .Posts
                .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
                .Where(p => p.ThreadId == id)
                .Take(BatchSize)
                .ExecuteDeleteAsync(cancellationToken);

            logger.LogInformation(
                "Deleted {DeletedRowsCount} posts. Scheduling next cleanup background job for thread {Id}",
                deletedRowsCount,
                id
            );

            backgroundJobClient.Enqueue<IThreadCleanupJob>(job => job.CleanupThreadAsync(id, CancellationToken.None));
            return;
        }

        logger.LogInformation("No posts to delete, deleting the subscriptions");

        var subscriptionsExist = await dbContext
            .ThreadSubscriptions
            .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
            .AnyAsync(s => s.ThreadId == id, cancellationToken);

        if (subscriptionsExist)
        {
            var deletedRowsCount = await dbContext
                .ThreadSubscriptions
                .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
                .Where(s => s.ThreadId == id)
                .Take(BatchSize)
                .ExecuteDeleteAsync(cancellationToken);

            logger.LogInformation(
                "Deleted {DeletedRowsCount} thread subscriptions. Scheduling next cleanup background job for thread {Id}",
                deletedRowsCount,
                id
            );

            backgroundJobClient.Enqueue<IThreadCleanupJob>(job => job.CleanupThreadAsync(id, CancellationToken.None));
            return;
        }

        logger.LogInformation("No subscriptions to delete for thread {Id}, deleting the thread itself", id);

        var deletedRows = await dbContext
            .Threads
            .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
            .Where(t => t.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    
        if (deletedRows == 1)
            logger.LogInformation("Thread {Id} deleted successfully", id);
        else
            logger.LogWarning("No thread was deleted");
    }
}
