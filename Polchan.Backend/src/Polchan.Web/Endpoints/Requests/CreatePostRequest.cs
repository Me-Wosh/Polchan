using System.ComponentModel.DataAnnotations;

namespace Polchan.Web.Endpoints.Requests;

public record CreatePostRequest
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    [Range(0, 5, ErrorMessage = "A maximum of 5 images can be uploaded")]
    public List<IFormFile>? Images { get; init; }
}
