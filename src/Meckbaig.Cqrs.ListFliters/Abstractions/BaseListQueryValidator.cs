using AutoMapper;
using FluentValidation;
using Meckbaig.Cqrs.Dto.Abstractions;
using Meckbaig.Cqrs.EntityFrameworkCore.Abstractons.Entities;
using Meckbaig.Cqrs.ListFliters.Attrubutes;
using Meckbaig.Cqrs.ListFliters.Extensions;
using Meckbaig.Cqrs.ListFliters.Models;
using System.Linq.Expressions;

namespace Meckbaig.Cqrs.ListFliters.Abstractions;

public abstract class BaseListQueryValidator<TQuery, TResponseList, TDestintaion, TSource> : AbstractValidator<TQuery>
	where TQuery : BaseListQuery<TResponseList>
	where TResponseList : BaseListQueryResponse<TDestintaion>
	where TDestintaion : class, IBaseDto
	where TSource : class, IEntityWithId
{
	public BaseListQueryValidator(IMapper mapper)
	{
		RuleFor(x => x.Skip).GreaterThanOrEqualTo(0);
		RuleFor(x => x.Take).GreaterThanOrEqualTo(0);
		RuleForEach(x => x.Filters).MinimumLength(3)
			.ValidateFilterParsing<TQuery, TResponseList, TDestintaion, TSource>(mapper);
		RuleForEach(x => x.OrderBy).MinimumLength(1)
			.ValidateSortParsing<TQuery, TResponseList, TDestintaion, TSource>(mapper);
	}
}

public static class BaseJournalQueryFilterValidatorExtension
{
	public static IRuleBuilderOptions<TQuery, string> ValidateFilterParsing
		<TQuery, TResponseList, TDestintaion, TSource>
		(this IRuleBuilderOptions<TQuery, string> ruleBuilder, IMapper mapper)
		where TQuery : BaseListQuery<TResponseList>
		where TResponseList : BaseListQueryResponse<TDestintaion>
		where TDestintaion : class, IBaseDto
		where TSource : class, IEntityWithId
	{
		string key = string.Empty;
		ruleBuilder = ruleBuilder
			.Must((query, filter) => PropertyExists<TDestintaion>(filter, mapper.ConfigurationProvider, ref key))
			.WithMessage(x => $"Property '{key.Print()}' does not exist");

		FilterExpression filterEx = null;
		ruleBuilder = ruleBuilder
			.Must((query, filter) => ExpressionIsValid<TDestintaion>(filter, mapper.ConfigurationProvider, ref filterEx))
			.WithMessage((query, filter) => $"{filter} - expression is undefined");

		ruleBuilder = ruleBuilder
			.Must((query, filter) => PropertyIsFilterable<TDestintaion, TSource>(filterEx, out key))
			.WithMessage((query, filter) => $"Property '{key.Print()}' is not filterable");

		string expressionErrorMessage = string.Empty;
		ruleBuilder = ruleBuilder
			.Must((query, filter) => CanCreateExpression<TQuery, TResponseList, TDestintaion, TSource>
					(query, filterEx, ref expressionErrorMessage))
			.WithMessage(x => expressionErrorMessage);

		return ruleBuilder;
	}

	private static bool PropertyExists<TDestintaion>(string filter, IConfigurationProvider provider, ref string key)
		where TDestintaion : class, IBaseDto
	{
		int expressionIndex;
		if (filter.Contains("!:"))
			expressionIndex = filter.IndexOf("!:");
		else if (filter.Contains(':'))
			expressionIndex = filter.IndexOf(":");
		else
			return true;
		string[] keySegments = filter[..expressionIndex].ToPropetyFormat().Split('.');
		Type type = typeof(TDestintaion);
		foreach (var segment in keySegments)
		{
			key = segment;
			string? endPoint = EntityFrameworkFiltersExtensions
				.GetExpressionEndpoint(key, provider, type, out Type nextType);
			if (endPoint == null)
				return false;
			type = nextType;
		}
		return true;
	}

	private static bool ExpressionIsValid<TDestintaion>
		(string filter, IConfigurationProvider provider, ref FilterExpression filterEx)
		where TDestintaion : class, IBaseDto
	{
		filterEx = EntityFrameworkFiltersExtensions.GetFilterExpression<TDestintaion>(filter, provider);
		if (filterEx?.ExpressionType == FilterExpressionType.Undefined)
			return false;
		return true;
	}

	private static bool PropertyIsFilterable<TDestintaion, TSource>
		(FilterExpression filterEx, out string key)
		where TDestintaion : IBaseDto
		where TSource : class, IEntityWithId
	{
		key = null;
		if (filterEx == null ||
			filterEx.EndPoint == null ||
			filterEx.ExpressionType == FilterExpressionType.Undefined)
			return true;
		return EntityFrameworkFiltersExtensions
			.TryGetFilterAttributes<TDestintaion>(filterEx, out key);
	}

	private static bool CanCreateExpression<TQuery, TResponseList, TDestintaion, TSource>
		(TQuery query, FilterExpression? filterEx, ref string errorMessage)
		where TQuery : BaseListQuery<TResponseList>
		where TResponseList : BaseListQueryResponse<TDestintaion>
		where TDestintaion : IBaseDto
		where TSource : class, IEntityWithId
	{

		if (filterEx == null ||
			filterEx.ExpressionType == FilterExpressionType.Undefined ||
			filterEx.CompareMethod == CompareMethod.Undefined)
		{
			return true;
		}
		if (filterEx.CompareMethod == CompareMethod.Custom)
		{
			query.AddPendingFilterExpression(filterEx);
			return true;
		}
		try
		{
			if (!EntityFrameworkFiltersExtensions.TryGetLinqExpression(filterEx, out var expression))
			{
				errorMessage = $"Compare method '{filterEx.CompareMethod}' may be not valid in provided context.";
				return false;
			}
			query.AddFilterExpression(expression);
			return true;
		}
		catch (Exception ex)
		{
			errorMessage = ex.Message;
			return false;
		}
	}
}

public static class BaseJournalQuerySortValidatorExtension
{
	public static IRuleBuilderOptions<TQuery, string> ValidateSortParsing
		<TQuery, TResponseList, TDestintaion, TSource>
		(this IRuleBuilderOptions<TQuery, string> ruleBuilder, IMapper mapper)
		where TQuery : BaseListQuery<TResponseList>
		where TResponseList : BaseListQueryResponse<TDestintaion>
		where TDestintaion : IBaseDto
		where TSource : class, IEntityWithId
	{
		string key = string.Empty;
		string? endPoint = null;
		ruleBuilder = ruleBuilder
			.Must((query, filter) => PropertyExists<TSource, TDestintaion>(filter, mapper.ConfigurationProvider, ref key, out endPoint))
			.WithMessage(x => $"Property '{key.Print()}' does not exist");

		OrderByExpression orderByEx = null;
		ruleBuilder = ruleBuilder
			.Must((query, filter) =>
			{
				if (endPoint == null)
					return false;
				return ExpressionIsValid<TQuery, TResponseList, TDestintaion, TSource>
					(filter, mapper.ConfigurationProvider, out orderByEx);
			})
			.WithMessage((query, filter) => $"{filter} - expression is undefined");

		string expressionErrorMessage = string.Empty;
		ruleBuilder = ruleBuilder
			.Must((query, filter) => CanCreateExpression<TQuery, TResponseList, TDestintaion, TSource>
					(query, orderByEx, ref expressionErrorMessage))
			.WithMessage(x => expressionErrorMessage);

		return ruleBuilder;
	}

	private static bool PropertyExists<TSource, TDestintaion>(
		string filter,
		IConfigurationProvider provider,
		ref string key,
		out string? endPoint)
	{
		if (filter.Contains(' '))
			key = filter[..filter.IndexOf(' ')].ToPascalCase();
		else
			key = filter.ToPascalCase();
		endPoint = EntityFrameworkOrderByExtensions
			.GetExpressionEndpoint<TSource, TDestintaion>(key, provider);
		if (endPoint == null)
			return false;
		return true;
	}

	private static bool ExpressionIsValid
		<TQuery, TResponseList, TDestintaion, TSource>
		(string filter, IConfigurationProvider provider, out OrderByExpression orderByEx)
		where TQuery : BaseListQuery<TResponseList>
		where TResponseList : BaseListQueryResponse<TDestintaion>
		where TDestintaion : IBaseDto
		where TSource : class, IEntityWithId
	{
		orderByEx = EntityFrameworkOrderByExtensions.GetOrderByExpression<TSource, TDestintaion>(filter, provider);
		if (orderByEx?.ExpressionType == OrderByExpressionType.Undefined)
			return false;
		return true;
	}

	private static bool CanCreateExpression<TQuery, TResponseList, TDestintaion, TSource>
		(TQuery query, OrderByExpression? orderByEx, ref string errorMessage)
		where TQuery : BaseListQuery<TResponseList>
		where TResponseList : BaseListQueryResponse<TDestintaion>
		where TDestintaion : IBaseDto
		where TSource : class, IEntityWithId
	{

		if (orderByEx == null ||
			orderByEx.ExpressionType == OrderByExpressionType.Undefined)
		{
			return true;
		}
		try
		{
			if (!EntityFrameworkOrderByExtensions.TryGetLinqExpression<TSource>(orderByEx, out Expression expression))
				return false;
			query.AddOrderExpression(expression);
			return true;
		}
		catch (Exception ex)
		{
			errorMessage = ex.Message;
			return false;
		}
	}
}
