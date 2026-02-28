using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Polchan.Application.Pagination;
using Polchan.Application.Posts;

namespace Polchan.Web.Endpoints;

public class CommentEndpoints : IEndpointGroup
{
    public void MapEndpoints(RouteGroupBuilder builder)
    {
        var group = builder
            .MapGroup("/comments")
            .RequireAuthorization();

        group.MapGet("/", async Task<Result<PaginatedList<CommentListItemResponse>>> (
            [FromQuery] Guid postId,
            [FromQuery] PaginationQuery paginationQuery,
            IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            return await mediator.Send(new GetAllCommentsByPostIdQuery(postId, paginationQuery), cancellationToken);
        });

        group.MapPost("/", async Task<Result<Unit>> (
            [FromQuery] Guid postId,
            [FromBody] CreateCommentCommand command,
            IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            return await mediator.Send(command with { PostId = postId }, cancellationToken);
        });

        group.MapPut("/{commentId:guid}", async Task<Result<Unit>> (
            [FromRoute] Guid commentId,
            [FromBody] UpdateCommentCommand command,
            IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            return await mediator.Send(command with { Id = commentId }, cancellationToken);
        });
    }
}
