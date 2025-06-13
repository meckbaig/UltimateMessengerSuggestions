namespace Meckbaig.Cqrs.ListFliters.ListFilters;

public interface IEntityFrameworkExpression<T> where T : Enum
{
	public string? Key { get; set; }
	public string? EndPoint { get; set; }
	public T ExpressionType { get; set; }
}
