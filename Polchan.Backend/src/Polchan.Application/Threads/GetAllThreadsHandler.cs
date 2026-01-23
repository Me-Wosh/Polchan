using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Polchan.Application.Threads.Responses;
using Polchan.Infrastructure;
using Polchan.Infrastructure.Pagination;
using Polchan.Shared.MediatR;

namespace Polchan.Application.Threads;

public record GetAllThreadsQuery([Required] PaginationQuery PaginationQuery) : IQuery<List<ThreadResponse>>;

public class GetAllThreadsHandler(PolchanDbContext dbContext) : IQueryHandler<GetAllThreadsQuery, List<ThreadResponse>>
{
    public async Task<Result<List<ThreadResponse>>> Handle(GetAllThreadsQuery query, CancellationToken cancellationToken)
    {
        return await dbContext
            .Threads
            .ApplyPagination(query.PaginationQuery)
            .Select(t => new ThreadResponse(t.Id, t.Name, Enum.GetName(t.Category)!))
            .ToListAsync(cancellationToken);
    }
}
