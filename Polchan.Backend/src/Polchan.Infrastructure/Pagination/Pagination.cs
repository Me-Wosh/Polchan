using System.Linq.Expressions;
using Polchan.Core;

namespace Polchan.Infrastructure.Pagination;

public record OrderBy(string Column, bool Ascending = true);

public record PaginationQuery(int PageNumber = 1, int PageSize = 10, OrderBy? OrderBy = null);

public static class Pagination
{
    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, PaginationQuery paginationQuery)
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

        var pageNumber = Math.Max(paginationQuery.PageNumber, 1);
        var pageSize = Math.Max(paginationQuery.PageSize, 0);

        query = query.Skip((pageNumber - 1) * pageSize);
        query = query.Take(pageSize);

        return query;
    }
}
