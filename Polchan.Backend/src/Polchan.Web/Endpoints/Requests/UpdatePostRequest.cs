using System.ComponentModel.DataAnnotations;

namespace Polchan.Web.Endpoints.Requests;

public record UpdatePostRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    [Range(0, 5, ErrorMessage = "A maximum of 5 images can be deleted")]
    public List<Guid>? ImageIdsToRemove { get; init; }
    [Range(0, 5, ErrorMessage = "A maximum of 5 images can be uploaded")]
    public List<IFormFile>? ImagesToAdd { get; init; }
}