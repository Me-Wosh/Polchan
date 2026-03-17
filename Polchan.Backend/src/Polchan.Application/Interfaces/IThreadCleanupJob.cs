namespace Polchan.Application.Interfaces;

public interface IThreadCleanupJob
{
    Task CleanupThreadAsync(Guid id, CancellationToken cancellationToken);
}
