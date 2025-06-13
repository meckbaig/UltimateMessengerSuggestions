using AutoMapper;
using Meckbaig.Cqrs.Dto.Abstractions;
using Meckbaig.Cqrs.Dto.Extensions;
using Meckbaig.Cqrs.EntityFrameworkCore.Abstractons.Entities;
using Meckbaig.Cqrs.ListFliters.ListFilters;

namespace Meckbaig.Cqrs.ListFliters.Models;

public record OrderByExpression : IEntityFrameworkExpression<OrderByExpressionType>
{
	public string? Key { get; set; }
	public string? EndPoint { get; set; }
	public OrderByExpressionType ExpressionType { get; set; }

	public static OrderByExpression Initialize<TSource, TDestintaion>(string filter, IConfigurationProvider provider)
		where TSource : class, IEntityWithId
		where TDestintaion : IBaseDto
	{
		var f = new OrderByExpression();

		if (!filter.Contains(' '))
		{
			f.Key = filter.ToPascalCase();
			f.EndPoint = DtoExtension.GetSource<TSource, TDestintaion>(f.Key, provider);
			f.ExpressionType = OrderByExpressionType.Ascending;
		}
		else if (filter[(filter.IndexOf(' ') + 1)..] == "desc")
		{
			f.Key = filter[..filter.IndexOf(' ')].ToPascalCase();
			f.EndPoint = DtoExtension.GetSource<TSource, TDestintaion>(f.Key, provider);
			f.ExpressionType = OrderByExpressionType.Descending;
		}
		else
			f.ExpressionType = OrderByExpressionType.Undefined;
		return f;
	}
}

public enum OrderByExpressionType
{
	Ascending, Descending, Undefined = -1
}
