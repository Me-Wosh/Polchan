namespace Polchan.Core.Threads.Entities;

public class ThreadSubscriptions : BaseEntity
{
    public Guid SubscriberId { get; private set; }
    public Guid ThreadId { get; private set; }

    private ThreadSubscriptions() { }
}
