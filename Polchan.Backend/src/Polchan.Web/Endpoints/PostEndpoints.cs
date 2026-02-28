using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Polchan.Application.Files;
using Polchan.Application.Pagination;
using Polchan.Application.Posts;
using Polchan.Web.Endpoints.Requests;

namespace Polchan.Web.Endpoints;

public class PostEndpoints : IEndpointGroup
{
    public void MapEndpoints(RouteGroupBuilder builder)
    {
        var group = builder
            .MapGroup("/posts")
            .RequireAuthorization();

        group.MapGet("/", async Task<Result<PaginatedList<PostListItemResponse>>> (
            [FromQuery] Guid threadId,
            [FromQuery] PaginationQuery paginationQuery,
            IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            return await mediator.Send(new GetAllPostsByThreadIdQuery(threadId, paginationQuery), cancellationToken);
        });

        group.MapPost("/", async Task<Result<Unit>> (
            [FromQuery] Guid threadId,
            [FromForm] CreatePostRequest request,
            IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            var images = request.Images ?? [];

            if (images.Count > 5)
                return Result.Invalid(new ValidationError("A maximum of 5 images can be uploaded"));

            if (images.Any(i => !i.ContentType.StartsWith("image/")))
                return Result.Invalid(new ValidationError("Only images can be uploaded"));

            var fileUploads = new List<FileUpload>();

            try
            {
                foreach (var image in images)
                    fileUploads.Add(new FileUpload(image.OpenReadStream(), image.FileName, image.ContentType));

                return await mediator.Send(
                    new CreatePostCommand(threadId, request.Title, request.Description, fileUploads),
                    cancellationToken
                );
            }

            finally
            {
                foreach (var fileUpload in fileUploads)
                    await fileUpload.Stream.DisposeAsync();
            }
        });

        group.MapPut("/{postId:guid}", async Task<Result<Unit>> (
            [FromRoute] Guid postId,
            [FromForm] UpdatePostRequest request,
            IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            var imagesToAdd = request.ImagesToAdd ?? [];

            if (imagesToAdd.Count > 5)
                return Result.Invalid(new ValidationError("A maximum of 5 images can be uploaded"));

            if (imagesToAdd.Any(i => !i.ContentType.StartsWith("image/")))
                return Result.Invalid(new ValidationError("Only images can be uploaded"));

            var fileUploads = new List<FileUpload>();

            try
            {
                foreach (var image in imagesToAdd)
                    fileUploads.Add(new FileUpload(image.OpenReadStream(), image.FileName, image.ContentType));

                return await mediator.Send(
                    new UpdatePostCommand(postId, request.Title, request.Description, request.ImageIdsToRemove, fileUploads),
                    cancellationToken
                );
            }

            finally
            {
                foreach (var fileUpload in fileUploads)
                    await fileUpload.Stream.DisposeAsync();
            }
        });
    }
}
