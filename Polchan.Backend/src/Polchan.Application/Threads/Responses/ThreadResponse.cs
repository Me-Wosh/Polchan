namespace Polchan.Application.Threads.Responses;

public record ThreadResponse
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Category { get; init; }
}
