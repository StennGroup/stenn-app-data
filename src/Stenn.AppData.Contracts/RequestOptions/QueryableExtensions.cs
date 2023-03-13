using System.Linq;

namespace Stenn.AppData.Contracts.RequestOptions
{
    public static class QueryableExtensions
    {
        public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> source, Filter? filter)
        {
            if (filter is null) return source;

            var filterExpression = new FilterExpressionBuilder<TSource>().GetFilterExpression(filter);

            return source.Where(filterExpression);
        }

        public static IQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> source, SortOptions? sortOptions)
        {
            if (sortOptions is null) return source;

            bool initialSort = true;
            foreach (var item in sortOptions)
            {
                source = source.ApplyOrder(item.FieldName, item.SortDirection == SortDirection.Descending, initialSort);
                initialSort = false;
            }

            return source;
        }

        public static IQueryable<TSource> Paginate<TSource>(this IQueryable<TSource> source, PagingOptions? pagingOptions)
        {
            if (pagingOptions is null) return source;

            source = source
                .Skip((pagingOptions.PageNumber - 1) * pagingOptions.PageSize)
                .Take(pagingOptions.PageSize);

            return source;
        }

        public static IQueryable<TSource> ApplyOptions<TSource>(this IQueryable<TSource> source, IAppDataRequestOptions requestOptions)
        {
            if (requestOptions is null) return source;

            return source
                .Where(requestOptions.Filter)
                .OrderBy(requestOptions.SortOptions)
                .Paginate(requestOptions.Paging);
        }
    }
}
