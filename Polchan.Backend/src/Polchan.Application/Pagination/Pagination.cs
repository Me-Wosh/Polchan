using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Polchan.Core;

namespace Polchan.Application.Pagination;

public static class Pagination
{
    public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> query,
        PaginationQuery paginationQuery,
        CancellationToken cancellationToken
    )
    {
        if (paginationQuery.OrderBy is null)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, nameof(BaseEntity.Id));
            var boxedProperty = Expression.Convert(property, typeof(object));
            var lambda = Expression.Lambda<Func<T, object>>(boxedProperty, parameter);
            
            query = query.OrderBy(lambda);
        }

        else
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, paginationQuery.OrderBy.Column);
            var boxedProperty = Expression.Convert(property, typeof(object));
            var lambda = Expression.Lambda<Func<T, object>>(boxedProperty, parameter);
            
            query = paginationQuery.OrderBy.Ascending
                ? query.OrderBy(lambda)
                : query.OrderByDescending(lambda);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var pageNumber = Math.Max(paginationQuery.PageNumber, 1);
        var pageSize = Math.Min(Math.Max(paginationQuery.PageSize, 0), 100);

        query = query.Skip((pageNumber - 1) * pageSize);
        query = query.Take(pageSize);

        var items = await query.ToListAsync(cancellationToken);

        return new PaginatedList<T>(items, totalCount);
    }
}

public record PaginatedList<T>(IEnumerable<T> Items, int TotalCount);

public record PaginationQuery(int PageNumber = 1, int PageSize = 10, OrderBy? OrderBy = null) : IParsable<PaginationQuery>
{
    // format of s parameter:
    // "pageNumber:1;pageSize:10;orderBy:{column:topic,ascending:false}"
    // "pageNumber:1;pageSize:10"

    public static PaginationQuery Parse(string s, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, out var result))
            throw new FormatException("Invalid pagination query format");

        return result;
    }

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out PaginationQuery result
    )
    {
        result = null;

        if (string.IsNullOrEmpty(s))
            return false;

        var parts = s.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        var pageNumberString = parts
            .Where(p => p.StartsWith("pageNumber", StringComparison.OrdinalIgnoreCase))
            .Select(p => 
            {
                var split = p.Split(':', 2);
                return split.Length == 2 ? split[1] : string.Empty;
            });

        if (pageNumberString.Count() != 1 || !int.TryParse(pageNumberString.First(), out var pageNumber))
            return false;
        
        var pageSizeString = parts
            .Where(p => p.StartsWith("pageSize", StringComparison.OrdinalIgnoreCase))
            .Select(p => 
            {
                var split = p.Split(':', 2);
                return split.Length == 2 ? split[1] : string.Empty;
            });
        
        if (pageSizeString.Count() != 1 || !int.TryParse(pageSizeString.First(), out var pageSize))
            return false;

        var orderByPart = parts
            .Where(p => p.StartsWith("orderBy", StringComparison.OrdinalIgnoreCase))
            .Select(p => 
            {
                var split = p.Split(':', 2);
                return split.Length == 2 ? split[1] : string.Empty;
            });

        if (orderByPart.Count() > 1)
            return false;

        OrderBy? orderBy = null;

        if (orderByPart.Any())
        {
            var orderByBody = orderByPart.First();

            if (orderByBody.StartsWith('{') && orderByBody.EndsWith('}'))
                orderByBody = orderByBody[1..^1];
            else
                return false;
            
            var orderByParameters = orderByBody
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            
            var column = orderByParameters
                .Where(p => p.StartsWith("column", StringComparison.OrdinalIgnoreCase))
                .Select(p => 
                {
                    var split = p.Split(':', 2);
                    return split.Length == 2 ? split[1] : string.Empty;
                });

            if (column.Count() != 1 || string.IsNullOrEmpty(column.First()))
                return false;
            
            var ascendingString = orderByParameters
                .Where(p => p.StartsWith("ascending", StringComparison.OrdinalIgnoreCase))
                .Select(p => 
                {
                    var split = p.Split(':', 2);
                    return split.Length == 2 ? split[1] : string.Empty;
                });

            if (ascendingString.Count() != 1 || !bool.TryParse(ascendingString.First(), out var ascending))
                return false;
            
            orderBy = new OrderBy(column.First(), ascending);
        }

        result = new PaginationQuery(pageNumber, pageSize, orderBy);
        return true;
    }
}

public record OrderBy(string Column, bool Ascending = true);
