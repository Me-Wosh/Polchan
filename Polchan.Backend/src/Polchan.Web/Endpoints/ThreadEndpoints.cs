using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Polchan.Application.Threads;
using Polchan.Application.Threads.Responses;

namespace Polchan.Web.Endpoints;

public static class ThreadEndpoints
{
    public static void MapThreadEndpoints(this RouteGroupBuilder builder)
    {
        var group = builder.MapGroup("/threads");

        group.RequireAuthorization();

        group.MapGet("/", async Task<Result<List<ThreadResponse>>> (
            [FromBody] GetAllThreadsQuery query,
            IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            return await mediator.Send(query, cancellationToken);
        });

        group.MapPost("/", async Task<Result<Unit>> (
            [FromBody] CreateThreadCommand command,
            IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            return await mediator.Send(command, cancellationToken);
        });

        group.MapGet("/user-subscribed", async Task<Result<List<ThreadResponse>>> (
            [FromBody] GetUserSubscribedThreadsQuery query,
            IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            return await mediator.Send(query, cancellationToken);
        });

        group.MapPatch("/{id:guid}", async Task<Result<Unit>> (
            [FromRoute] Guid id,
            [FromBody] UpdateThreadCommand command,
            IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            return await mediator.Send(command with { Id = id }, cancellationToken);
        });

        group.MapPost("/{id:guid}/subscribe", async Task<Result<Unit>> (
            [FromRoute] Guid id,
            IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            return await mediator.Send(new SubscribeThreadCommand(id), cancellationToken);
        });

        group.MapPost("{id:guid}/unsubscribe", async Task<Result<Unit>> (
            [FromRoute] Guid id,
            IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            return await mediator.Send(new UnsubscribeThreadCommand(id), cancellationToken);
        });

        group.MapGet("/categories", async Task<Result<IEnumerable<ThreadCategoryResponse>>> (
            IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            return await mediator.Send(new GetAllThreadCategoriesQuery(), cancellationToken);
        });
    }
}
