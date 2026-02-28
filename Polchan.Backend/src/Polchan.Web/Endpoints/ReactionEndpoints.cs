using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Polchan.Application.Posts;

namespace Polchan.Web.Endpoints;

public class ReactionEndpoints : IEndpointGroup
{
    public void MapEndpoints(RouteGroupBuilder builder)
    {
        var group = builder
            .MapGroup("/reactions")
            .RequireAuthorization();

        group.MapPut("/", async Task<Result<Unit>> (
            [FromBody] AddReactionCommand command,
            IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            return await mediator.Send(command, cancellationToken);
        });

        group.MapDelete("/{id:guid}", async Task<Result<Unit>> (
            [FromRoute] Guid id,
            [FromBody] RemoveReactionCommand command,
            IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            return await mediator.Send(command with { Id = id }, cancellationToken);
        });
    }
}
