namespace Polchan.Web.Endpoints.Requests;

public record CreatePostRequest
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public List<IFormFile>? Images { get; init; }
}
