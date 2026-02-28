using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Polchan.Application.Pagination;
using Polchan.Core.Interfaces;
using Polchan.Core.Posts.Enums;
using Polchan.Shared.MediatR;

namespace Polchan.Application.Posts;

public record GetAllCommentsByPostIdQuery(
    [Required] Guid PostId,
    [Required] PaginationQuery PaginationQuery
) : IQuery<PaginatedList<CommentListItemResponse>>;

public record CommentListItemResponse
{
    public Guid Id { get; init; }
    public required string Content { get; init; }
    public int UpvotesCount { get; init; }
    public int DownvotesCount { get; init; }
}

public class GetAllCommentsByPostIdHandler(
    IPolchanDbContext dbContext
) : IQueryHandler<GetAllCommentsByPostIdQuery, PaginatedList<CommentListItemResponse>>
{
    public async Task<Result<PaginatedList<CommentListItemResponse>>> Handle(
        GetAllCommentsByPostIdQuery query,
        CancellationToken cancellationToken
    )
    {
        return await dbContext
            .Comments
            .AsNoTracking()
            .Include(c => c.Reactions)
            .Where(c => c.PostId == query.PostId)
            .Select(c => new CommentListItemResponse
            {
                Id = c.Id,
                Content = c.Content,
                UpvotesCount = c.Reactions.Count(r => r.ReactionType == ReactionType.Upvote),
                DownvotesCount = c.Reactions.Count(r => r.ReactionType == ReactionType.Downvote)
            })
            .ToPaginatedListAsync(query.PaginationQuery, cancellationToken);
    }
}
