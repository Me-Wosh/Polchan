using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Polchan.Application.Interfaces;
using Polchan.Application.Pagination;
using Polchan.Application.Threads.Responses;
using Polchan.Shared.MediatR;

namespace Polchan.Application.Threads;

public record GetAllThreadsQuery([Required] PaginationQuery PaginationQuery) : IQuery<PaginatedList<ThreadResponse>>;

public class GetAllThreadsHandler(IPolchanDbContext dbContext) : IQueryHandler<GetAllThreadsQuery, PaginatedList<ThreadResponse>>
{
    public async Task<Result<PaginatedList<ThreadResponse>>> Handle(GetAllThreadsQuery query, CancellationToken cancellationToken)
    {
        return await dbContext
            .Threads
            .AsNoTracking()
            .Select(t => new ThreadResponse
            {
                Id = t.Id,
                Name = t.Name,
                Category = t.Category.ToString()
            })
            .ToPaginatedListAsync(query.PaginationQuery, cancellationToken);
    }
}
