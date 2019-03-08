using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WebAPIs.Models;


namespace WebAPIs.Data
{
    public static class DataSort
    {

        public static IQueryable<T> SortBy<T>(this IQueryable<T> source, string sortColumn, bool? sortOrder)
        {
            //var type = typeof(T);
            //var param = Expression.Parameter(type, "x");
            //var property = Expression.Property(param, propertyName);
            //var sort = Expression.Lambda(property, param);
            //var methodName = dataHelper.SortOrder == true ? "OrderBy" : "OrderByDescending";
            //Expression<Func<IQueryable<T>>> sortMethod = () => source.OrderBy<T, object>(k => null);
            //var methodCallExpression = sortMethod.Body as MethodCallExpression;
            //var method = methodCallExpression.Method.GetGenericMethodDefinition();
            //var genericSortMethod = method.MakeGenericMethod(type, property.Type);
            //var orderedQuery = (IOrderedQueryable<T>)genericSortMethod.Invoke(source, new object[] { source, sort });
            //return orderedQuery;

            var type = typeof(T);
            var property = type.GetProperty(sortColumn);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);
            var typeArguments = new Type[] { type, property.PropertyType };
            var methodName = sortOrder == true ? "OrderBy" : "OrderByDescending";
            var resultExp = Expression.Call(typeof(Queryable), methodName, typeArguments, source.Expression, Expression.Quote(orderByExp));
            var query = source.Provider.CreateQuery<T>(resultExp);
            return query;
        }
    }

    public static class DataCount
    {
        public static IQueryable<T> Page<T>(this IQueryable<T> pageQuery, int page, int size)
        {
            var type = typeof(T);
            if (page == 0 & size == 0)
            {
                return pageQuery;
            }
            int totalCount = pageQuery.Count();
            var skip = (page - 1) * size;
            var take = size;
            var pageOfResults = pageQuery.Skip(skip).Take(take);
            return pageOfResults;
        }
    }
}
