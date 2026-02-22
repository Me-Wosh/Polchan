namespace Polchan.Shared.Options;

public class StorageOptions
{
    public required Local Local { get; init; }
}

public class Local
{
    public required string BasePath { get; init; }
}
