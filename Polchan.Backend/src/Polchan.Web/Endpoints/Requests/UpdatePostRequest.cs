namespace Polchan.Web.Endpoints.Requests;

public record UpdatePostRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public List<Guid>? ImageIdsToRemove { get; init; }
    public List<IFormFile>? ImagesToAdd { get; init; }
}