using System.Linq.Expressions;
using Bridge.Domain;
using Bridge.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Infrastructure;

public static class QueryableExtensions
{
    public static async Task<T> GetAsync<T>(this IQueryable<T> queryable, CancellationToken cancellationToken = default)
        where T : class
    {
        var entity = await queryable.FirstOrDefaultAsync(cancellationToken);
        if (entity == null)
        {
            throw new NotFoundException($"Entity not found: {typeof(T).Name}");
        }

        return entity;
    }

    public static Task<T> GetAsync<T>(this IQueryable<T> queryable,
        Expression<Func<T, bool>> expression,
        CancellationToken cancellationToken = default)
        where T : class
    {
        return queryable.Where(expression).GetAsync(cancellationToken);
    }

    public static async Task<Page<T>> Materialize<T>(this IQueryable<T> queryable,
        PaginatedRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(request.PageNumber, 1, nameof(request.PageNumber));
        var count = await queryable.CountAsync(cancellationToken);
        var collection = await queryable
            .Skip((request.PageNumber - 1) * request.ItemPerPage)
            .Take(request.ItemPerPage)
            .ToListAsync(cancellationToken);
        return new()
        {
            Items = collection,
            PageNumber = request.PageNumber,
            TotalSize = count
        };
    }
}