using Meckbaig.Cqrs.Abstractons;
using Meckbaig.Cqrs.ListFliters.Models;
using System.Linq.Expressions;

namespace Meckbaig.Cqrs.ListFliters.Abstractions;

public abstract record BaseListQuery<TResponse> : BaseRequest<TResponse>
	where TResponse : BaseResponse
{
	/// <summary>
	/// The number of items to skip in the result set. This is used for pagination.
	/// </summary>
	public abstract int Skip { get; init; }

	/// <summary>
	/// The number of items to take from the result set. This is used for pagination.
	/// </summary>
	public abstract int Take { get; init; }

	/// <summary>
	/// An array of filter strings to apply to the query. Each string should be a valid filter expression.
	/// </summary>
	public abstract string[]? Filters { get; init; }

	/// <summary>
	/// An array of order by strings to apply to the query. Each string should be a valid order by expression.
	/// </summary>
	public abstract string[]? OrderBy { get; init; }

	private readonly Dictionary<string, FilterExpression> _pendingFilterExpressions = [];
	private readonly List<Expression> _filterExpressions = [];
	private readonly List<Expression> _orderExpressions = [];

	private FilterExpression PopPendingFilterExpression(string key)
	{
		_pendingFilterExpressions.TryGetValue(key, out var filterExpression);
		if (filterExpression is null)
		{
			throw new KeyNotFoundException($"Filter expression with key '{key}' not found.");
		}
		_pendingFilterExpressions.Remove(key);
		return filterExpression;
	}

	public List<Expression> GetFilterExpressions()
		=> _filterExpressions;

	public List<Expression> GetOrderExpressions()
		=> _orderExpressions;

	public void AddPendingFilterExpression(FilterExpression filterExpression)
		=> _pendingFilterExpressions!.Add(filterExpression.Key, filterExpression);

	public void AddFilterExpression(string key, Func<FilterExpression, Expression> getTagsFilterExpression) 
		=> _filterExpressions.Add(getTagsFilterExpression(PopPendingFilterExpression(key)));

	public void AddFilterExpression(Expression expression)
		=> _filterExpressions!.Add(expression);

	public void AddOrderExpression(Expression expression)
		=> _orderExpressions!.Add(expression);
}
