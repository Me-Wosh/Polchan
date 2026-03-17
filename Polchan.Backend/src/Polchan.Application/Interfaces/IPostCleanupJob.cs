namespace Polchan.Application.Interfaces;

public interface IPostCleanupJob
{
    Task CleanupPostAsync(Guid id, CancellationToken cancellationToken);
}
