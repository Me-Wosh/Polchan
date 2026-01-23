using Ardalis.Result;
using Polchan.Core.Threads.Enums;
using Polchan.Shared.MediatR;

namespace Polchan.Application.Threads;

public record GetAllThreadCategoriesQuery : IQuery<IEnumerable<ThreadCategoryResponse>>;

public record ThreadCategoryResponse(int Id, string Name);

public class GetAllThreadCategoriesHandler : IQueryHandler<GetAllThreadCategoriesQuery, IEnumerable<ThreadCategoryResponse>>
{
    public async Task<Result<IEnumerable<ThreadCategoryResponse>>> Handle(
        GetAllThreadCategoriesQuery query,
        CancellationToken cancellationToken
    )
    {
        return await Task.FromResult(
            Result.Success(
                Enum
                    .GetValues<ThreadCategory>()
                    .Select(tc => new ThreadCategoryResponse((int)tc, tc.ToString()))
            )
        );
    }
}
