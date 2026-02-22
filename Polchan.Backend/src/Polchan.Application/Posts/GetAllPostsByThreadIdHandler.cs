using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Polchan.Application.Interfaces;
using Polchan.Application.Pagination;
using Polchan.Core.Posts.Enums;
using Polchan.Shared.MediatR;

namespace Polchan.Application.Posts;

public record GetAllPostsByThreadIdQuery(
    [Required] Guid ThreadId,
    [Required] PaginationQuery PaginationQuery
) : IQuery<PaginatedList<PostListResponse>>;

public record PostListResponse
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required IEnumerable<Guid> ImageIds { get; init; }
    public int UpvotesCounts { get; init; }
    public int DownvotesCount { get; init; }
    public int CommentsCount { get; init; }
}

public class GetAllPostsByThreadIdHandler(
    IPolchanDbContext dbContext
) : IQueryHandler<GetAllPostsByThreadIdQuery, PaginatedList<PostListResponse>>
{
    public async Task<Result<PaginatedList<PostListResponse>>> Handle(GetAllPostsByThreadIdQuery query, CancellationToken cancellationToken)
    {
        return await dbContext
            .Posts
            .AsNoTracking()
            .Include(p => p.Images)
            .Include(p => p.Reactions)
            .Include(p => p.Comments)
            .Where(p => p.ThreadId == query.ThreadId)
            .Select(p => new PostListResponse
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                ImageIds = p.Images.Select(r => r.Id),
                UpvotesCounts = p.Reactions.Count(r => r.ReactionType == ReactionType.Upvote),
                DownvotesCount = p.Reactions.Count(r => r.ReactionType == ReactionType.Downvote),
                CommentsCount = p.Comments.Count()
            })
            .ToPaginatedListAsync(query.PaginationQuery, cancellationToken);
    }
}
