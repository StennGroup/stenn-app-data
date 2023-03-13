using System;
using System.Linq;
using System.Linq.Expressions;

namespace Stenn.AppData.Contracts.RequestOptions
{
    public static class SortingExpressionBuilder
    {
        internal static IQueryable<TEntity> ApplyOrder<TEntity>(this IQueryable<TEntity> source, string orderByProperty, bool desc, bool initialSort)
        {
            string command;

            if (initialSort)
            {
                command = desc ? "OrderByDescending" : "OrderBy";
            }
            else
            {
                command = desc ? "ThenByDescending" : "ThenBy";
            }
            
            var type = typeof(TEntity);
            var property = type.GetProperty(orderByProperty)!;
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);
            var resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { type, property.PropertyType },
                                          source.Expression, Expression.Quote(orderByExpression));

            return source.Provider.CreateQuery<TEntity>(resultExpression);
        }
    }
}
