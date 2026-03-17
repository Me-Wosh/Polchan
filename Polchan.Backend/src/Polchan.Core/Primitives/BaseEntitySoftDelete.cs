namespace Polchan.Core.Primitives;

public abstract class BaseEntitySoftDelete : BaseEntity
{
    public bool SoftDeleted { get; protected set; }
}
