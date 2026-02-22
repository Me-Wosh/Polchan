using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Polchan.Application.Interfaces;
using Polchan.Application.Pagination;
using Polchan.Application.Threads.Responses;
using Polchan.Shared.MediatR;

namespace Polchan.Application.Threads;

public record GetUserSubscribedThreadsQuery([Required] PaginationQuery PaginationQuery) : IQuery<PaginatedList<ThreadResponse>>;

public class GetUserSubscribedThreadsHandler(
    IUserAccessor userAccessor,
    IPolchanDbContext dbContext
) : IQueryHandler<GetUserSubscribedThreadsQuery, PaginatedList<ThreadResponse>>
{
    public async Task<Result<PaginatedList<ThreadResponse>>> Handle(
        GetUserSubscribedThreadsQuery query,
        CancellationToken cancellationToken
    )
    {
        var userId = userAccessor.GetUserId();

        if (!userId.IsSuccess)
            return userId.Map();

        return await dbContext
            .Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .SelectMany(u => u.SubscribedThreads)
            .Select(t => new ThreadResponse
            {
                Id = t.Id,
                Name = t.Name,
                Category = t.Category.ToString()
            })
            .ToPaginatedListAsync(query.PaginationQuery, cancellationToken);
    }
}