using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Polchan.Application.Auth.Services;
using Polchan.Application.Threads.Responses;
using Polchan.Infrastructure;
using Polchan.Infrastructure.Pagination;
using Polchan.Shared.MediatR;

namespace Polchan.Application.Threads;

public record GetUserSubscribedThreadsQuery([Required] PaginationQuery PaginationQuery) : IQuery<List<ThreadResponse>>;

public class GetUserSubscribedThreadsHandler(
    IUserAccessor userAccessor,
    PolchanDbContext dbContext
) : IQueryHandler<GetUserSubscribedThreadsQuery, List<ThreadResponse>>
{
    public async Task<Result<List<ThreadResponse>>> Handle(
        GetUserSubscribedThreadsQuery query,
        CancellationToken cancellationToken
    )
    {
        var userId = userAccessor.GetUserId();

        if (!userId.IsSuccess)
            return userId.Map();

        return await dbContext
            .Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.SubscribedThreads)
            .ApplyPagination(query.PaginationQuery)
            .Select(t => new ThreadResponse(t.Id, t.Name, Enum.GetName(t.Category)!))
            .ToListAsync(cancellationToken);
    }
}