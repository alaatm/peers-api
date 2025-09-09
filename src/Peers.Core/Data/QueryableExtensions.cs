using System.Text.Json;
using System.Linq.Dynamic.Core;
using Peers.Core.Common;
using System.Globalization;
using System.Text;
using System.Diagnostics;

namespace Peers.Core.Data;

public static class QueryableExtensions
{
    private static readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    /// <summary>
    /// Applies the set of specified filters to a queryable.
    /// </summary>
    /// <typeparam name="T">The type of the queryable</typeparam>
    /// <param name="q">The queryable.</param>
    /// <param name="filters">The filters.</param>
    /// <returns></returns>
    public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> q, string? filters)
    {
        if (string.IsNullOrWhiteSpace(filters))
        {
            return q;
        }

        var filtersObj = JsonSerializer.Deserialize<Dictionary<string, string[]>>(filters, GlobalJsonOptions.Default) ?? [];

        foreach (var (key, value) in filtersObj)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException($"Query filter parsing error. One or more keys are empty.");
            }

            if (value.Length % 2 != 1)
            {
                throw new InvalidOperationException($"Query filter parsing error at key '{key}'. Invalid operations count.");
            }

            var iArg = 0;
            var args = new string[(value.Length / 2) + 1];
            var predicate = new StringBuilder();

            for (var i = 0; i < value.Length; i++)
            {
                var param = value[i];

                if (i % 2 == 1)
                {
                    if (param is not "and" and not "or")
                    {
                        throw new InvalidOperationException($"Query filter parsing error at key '{key}'. Invalid logic operator '{param}'.");
                    }

                    predicate.Append(_culture, $" {param} ");
                }
                else
                {
                    var match = RegexStatic.FilterRegex().Match(param);
                    if (!match.Success)
                    {
                        throw new InvalidOperationException($"Query filter parsing error at key '{key}'. Invalid operation '{param}'.");
                    }

                    var func = match.Groups["func"].Value;
                    var arg = match.Groups["arg"].Value;

                    var f = func switch
                    {
                        "startswith" => $"{key}.StartsWith(@{iArg})",
                        "contains" => $"{key}.Contains(@{iArg})",
                        "endswith" => $"{key}.EndsWith(@{iArg})",
                        "lt" => $"{key}<@{iArg}",
                        "le" => $"{key}<=@{iArg}",
                        "eq" => $"{key}==@{iArg}",
                        "ge" => $"{key}>=@{iArg}",
                        "gt" => $"{key}>@{iArg}",
                        "ne" => $"{key}!=@{iArg}",
                        _ => throw new UnreachableException(),
                    };

                    args[iArg++] = arg;
                    predicate.Append(f);
                }
            }

            q = q.Where(predicate.ToString(), args);
        }

        return q;
    }

    /// <summary>
    /// Applies the specified sorting to a queryable.
    /// </summary>
    /// <typeparam name="T">The type of the queryable</typeparam>
    /// <param name="q">The queryable.</param>
    /// <param name="sortField">The sort field.</param>
    /// <param name="sortOrder">The sort order.</param>
    /// <returns></returns>
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> q, string? sortField, string? sortOrder)
    {
        if (string.IsNullOrWhiteSpace(sortField))
        {
            return q;
        }

        sortOrder = sortOrder is "descend" or "desc"
            ? "desc"
            : "asc";

        return q.OrderBy($"{sortField} {sortOrder}");
    }

    /// <summary>
    /// Applies the specified paging to a queryable.
    /// </summary>
    /// <typeparam name="T">The type of the queryable</typeparam>
    /// <param name="q">The queryable.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns></returns>
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> q, int? page, int? pageSize)
    {
        const int DefaultPage = 1;
        const int DefaultPageSize = 15;

        page ??= DefaultPage;
        pageSize ??= DefaultPageSize;

        if (page < 1)
        {
            page = DefaultPage;
        }

        if (pageSize < 1)
        {
            pageSize = DefaultPageSize;
        }

        return q
            .Skip(pageSize.Value * (page.Value - 1))
            .Take(pageSize.Value);
    }
}

