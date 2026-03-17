using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polchan.Application.Interfaces;
using Polchan.Core.Interfaces;

namespace Polchan.Infrastructure.BackgroundJobs;

public class PostCleanupJob(
    ILogger<PostCleanupJob> logger,
    IPolchanDbContext dbContext,
    IBackgroundJobClient backgroundJobClient
) : IPostCleanupJob
{
    private const int BatchSize = 1000;

    public async Task CleanupPostAsync(Guid id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Initiating cleanup of post {Id}", id);

        var postExists = await dbContext
            .Posts
            .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
            .AnyAsync(p => p.Id == id, cancellationToken);

        if (!postExists)
        {
            logger.LogWarning("Post with Id {Id} not found", id);
            return;
        }

        var postReactionsExist = await dbContext
            .Reactions
            .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
            .AnyAsync(r => r.PostId == id, cancellationToken);

        if (postReactionsExist)
        {
            var deletedRowsCount = await dbContext
                .Reactions
                .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
                .Where(r => r.PostId == id)
                .Take(BatchSize)
                .ExecuteDeleteAsync(cancellationToken);

            logger.LogInformation(
                "Deleted {DeletedRowsCount} reactions. Scheduling next cleanup background job for post {Id}",
                deletedRowsCount,
                id
            );

            backgroundJobClient.Enqueue<IPostCleanupJob>(job => job.CleanupPostAsync(id, CancellationToken.None));
            return;
        }

        logger.LogInformation("No reactions to delete for post {Id}, initiating comment reactions cleanup", id);

        var commentReactionsExist = await dbContext
            .Reactions
            .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
            .AnyAsync(r => r.Comment != null && r.Comment.PostId == id, cancellationToken);

        if (commentReactionsExist)
        {
            var deletedRowsCount = await dbContext
                .Reactions
                .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
                .Where(r => r.Comment != null && r.Comment.PostId == id)
                .Take(BatchSize)
                .ExecuteDeleteAsync(cancellationToken);

            logger.LogInformation(
                "Deleted {DeletedRowsCount} comment reactions. Scheduling next cleanup background job for post {Id}",
                deletedRowsCount,
                id
            );

            backgroundJobClient.Enqueue<IPostCleanupJob>(job => job.CleanupPostAsync(id, CancellationToken.None));
            return;
        }

        logger.LogInformation("No reactions to delete for comments, initiating comments cleanup");

        var commentsExist = await dbContext
            .Comments
            .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
            .AnyAsync(c => c.PostId == id, cancellationToken);

        if (commentsExist)
        {
            var deletedRowsCount = await dbContext
                .Comments
                .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
                .Where(c => c.PostId == id)
                .Take(BatchSize)
                .ExecuteDeleteAsync(cancellationToken);

            logger.LogInformation(
                "Deleted {DeletedRowsCount} comments. Scheduling next cleanup background job for post {Id}",
                deletedRowsCount,
                id
            );

            backgroundJobClient.Enqueue<IPostCleanupJob>(job => job.CleanupPostAsync(id, CancellationToken.None));
            return;
        }        

        logger.LogInformation("No comments to delete for post {Id}, deleting the post itself", id);

        var deletedRows = await dbContext
            .Posts
            .IgnoreQueryFilters(["ExcludeSoftDeletedFilter"])
            .Where(p => p.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedRows == 1)
            logger.LogInformation("Post {Id} deleted successfully", id);
        else
            logger.LogWarning("No post was deleted");
    }
}
