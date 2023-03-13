using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Stenn.AppData.Contracts.RequestOptions
{
    public class FilterExpressionBuilder<T>
    {
        internal Expression<Func<T, bool>> GetFilterExpression(Filter filter)
        {
            if (filter == null) return (T) => true;
            var expressionParameter = Expression.Parameter(typeof(T));
            var expression = Expression.Lambda<Func<T, bool>>(GetExpressionForToken(filter.CurrentToken, expressionParameter), expressionParameter);
            return expression;
        }

        private Expression GetExpressionForToken(IToken requestCurrentToken, ParameterExpression expressionValueToCompare)
        {
            return requestCurrentToken switch
            {
                And => VisitAnd((And)requestCurrentToken, expressionValueToCompare),
                Or => VisitOr((Or)requestCurrentToken, expressionValueToCompare),
                Condition => VisitSimpleCondition((Condition)requestCurrentToken, expressionValueToCompare),
                ContainsCondition => VisitContainsCondition((ContainsCondition)requestCurrentToken, expressionValueToCompare),
                _ => throw new ArgumentOutOfRangeException(requestCurrentToken.ToString())
            };
        }

        private Expression VisitContainsCondition(ContainsCondition requestCurrentCondition, ParameterExpression expressionParameter)
        {
            var columnExpression = Expression.Property(expressionParameter, typeof(T).GetProperty(requestCurrentCondition.FieldName)!);

            object comapreToObject = requestCurrentCondition.RightValue;

            var arrayExpression = Expression.Constant(comapreToObject);
            var all = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static);
            var methodContains = all
                .Where(x => x.Name == nameof(Enumerable.Contains))
                .Single(x => x.GetParameters().Length == 2).MakeGenericMethod(comapreToObject.GetType().GetElementType()!);

            return Expression.Call(null, methodContains, arrayExpression, columnExpression);
        }

        private static Expression VisitSimpleCondition(Condition requestCurrentCondition,
            ParameterExpression expressionParameter)
        {
            var propertyInfo = typeof(T).GetProperty(requestCurrentCondition.FieldName);
            var columnExpression = Expression.Property(expressionParameter, propertyInfo!);

            object comapreToObject = requestCurrentCondition.RightValue;
            Expression compareTo;

            if (propertyInfo!.PropertyType != comapreToObject.GetType())
            {
                compareTo = Expression.Convert(Expression.Constant(comapreToObject), propertyInfo!.PropertyType);
            }
            else
            {
                compareTo = Expression.Constant(comapreToObject);
            }

            return requestCurrentCondition.ConditionType switch
            {
                ConditionType.Equals => Expression.Equal(columnExpression, compareTo),
                ConditionType.Greater => Expression.GreaterThan(columnExpression, compareTo),
                ConditionType.Less => Expression.LessThan(columnExpression, compareTo),
                _ => throw new ArgumentOutOfRangeException(requestCurrentCondition.ConditionType.ToString())
            };
        }

        private Expression VisitOr(Or requestOr, ParameterExpression expressionParameter)
        {
            return Expression.Or(GetExpressionForToken(requestOr.First, expressionParameter), GetExpressionForToken(requestOr.Second, expressionParameter));
        }

        private Expression VisitAnd(And requestAnd, ParameterExpression expressionParameter)
        {
            return Expression.And(GetExpressionForToken(requestAnd.First, expressionParameter), GetExpressionForToken(requestAnd.Second, expressionParameter));
        }
    }
}
