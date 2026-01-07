using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Polchan.Application.Auth;

namespace Polchan.Web.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this RouteGroupBuilder builder)
    {
        var group = builder.MapGroup("/auth");

        group.MapPost("/login", async Task<Result<string>> (
            [FromBody]LoginUserCommand command,
            IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            return await mediator.Send(command, cancellationToken);
        });

        group.MapPost("/register", async Task<Result<Unit>> (
            [FromBody]RegisterUserCommand command,
            IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            return await mediator.Send(command, cancellationToken);
        });

        group.MapPost("/refresh-tokens", async Task<Result<string>> (
            IMediator mediator,
            CancellationToken cancellationToken
        ) =>
        {
            return await mediator.Send(new RefreshTokensCommand(), cancellationToken);
        });
    }
}
