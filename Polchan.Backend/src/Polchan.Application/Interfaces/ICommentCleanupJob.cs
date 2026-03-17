namespace Polchan.Application.Interfaces;

public interface ICommentCleanupJob
{
    Task CleanupCommentAsync(Guid id, CancellationToken cancellationToken);
}
