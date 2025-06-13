using AutoMapper;
using FluentValidation;
using Meckbaig.Cqrs.Dto.Abstractions;
using Meckbaig.Cqrs.Dto.Extensions;
using Meckbaig.Cqrs.EntityFrameworkCore.Abstractons.Entities;
using Meckbaig.Cqrs.Extensions;
using Meckbaig.Cqrs.ListFliters.Attrubutes;
using Meckbaig.Cqrs.ListFliters.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

namespace Meckbaig.Cqrs.ListFliters.Extensions;

/// <summary>
/// Custom EF Core extencion class for dynamic filtering
/// </summary>
public static class EntityFrameworkFiltersExtensions
{
	/// <summary>
	/// Adds 'Where' statements using input filters and mapping engine
	/// </summary>
	/// <typeparam name="TSource">Source of DTO type</typeparam>
	/// <param name="source">Queryable source</param>
	/// <param name="filterExpressions">Array of filter expressions</param>
	/// <returns>An <typeparamref name="IQueryable"/> that contains filters</returns>
	public static IQueryable<TSource> AddFilters<TSource>
		(this IQueryable<TSource> source, List<Expression>? filterExpressions)
		where TSource : IEntityWithId
	{
		if (filterExpressions == null)
			return source;
		foreach (var expression in filterExpressions)
		{
			source = source.Where((Expression<Func<TSource, bool>>)expression);
		}
		return source;
	}

	/// <summary>
	/// Gets full endpoint route string.
	/// </summary>
	/// <param name="destinationPropertyName"></param>
	/// <param name="provider">Configuraion provider for performing maps.</param>
	/// <param name="destinationType">DTO type.</param>
	/// <param name="propertyType">Next property type, if property is collection; else null.</param>
	/// <returns>Returns endpoint if success, null if error</returns>
	public static string? GetExpressionEndpoint
		(string destinationPropertyName, IConfigurationProvider provider, Type destinationType, out Type propertyType)
	{
		propertyType = destinationType;
		DtoExtension.InvokeTryGetSource(
					destinationPropertyName,
					provider,
					ref propertyType,
					out string sourceSegment,
					out string errorMessage,
					throwException: false);
		if (propertyType.IsCollection())
			propertyType = propertyType.GetGenericArguments().Single();
		else
			propertyType = null;

		return sourceSegment;
	}

	/// <summary>
	/// Gets filter expression
	/// </summary>
	/// <typeparam name="TDestintaion">DTO type</typeparam>
	/// <param name="filter">Filter from client</param>
	/// <param name="provider">Configuraion provider for performing maps</param>
	/// <returns>Returns FilterExpression model if success, undefined FilterExpression
	/// if can not parse expression, null if error</returns>
	public static FilterExpression? GetFilterExpression
		<TDestintaion>
		(string filter, IConfigurationProvider provider)
		where TDestintaion : class, IBaseDto
	{
		try
		{
			return FilterExpression.Initialize<TDestintaion>(filter, provider);
		}
		catch (ValidationException)
		{
			return null;
		}
	}

	/// <summary>
	/// Gets filter attribute from property path.
	/// </summary>
	/// <typeparam name="TDestintaion">DTO type.</typeparam>
	/// <param name="propertyPath">Property path to get attribute from.</param>
	/// <returns>Returns FilterableAttribute models for full path if success, null if error.</returns>
	public static bool TryGetFilterAttributes<TDestintaion>
		(FilterExpression filterEx, out string key)
		where TDestintaion : IBaseDto
	{
		var tmpFilterEx = filterEx;
		Type nextSegmentType = typeof(TDestintaion);
		do
		{
			key = tmpFilterEx.Key;
			var prop = nextSegmentType.GetProperties().FirstOrDefault(p => p.Name == tmpFilterEx.Key)!;

			var attribute = (FilterableAttribute)prop.GetCustomAttributes(true)
				.FirstOrDefault(a => a.GetType() == typeof(FilterableAttribute))!;
			if (attribute == null)
				return false;

			tmpFilterEx.CompareMethod = attribute.CompareMethod;

			if (tmpFilterEx.CompareMethod == CompareMethod.Nested)
				nextSegmentType = prop.PropertyType.GetGenericArguments().Single();
			else
				nextSegmentType = prop.PropertyType;

			tmpFilterEx = tmpFilterEx.InnerFilterExpression;

		} while (tmpFilterEx != null);

		return true;
	}

	/// <summary>
	/// Gets filter expression for Where statement.
	/// </summary>
	/// <param name="filterEx">Filter expression.</param>
	/// <param name="expression">Filter expression if success, null if error.</param>
	/// <returns><see langword="true"/> if success, <see langword="false"/> if error.</returns>
	public static bool TryGetLinqExpression
		(FilterExpression filterEx, out Expression expression)
	{
		var param = Expression.Parameter(filterEx.EntityType, filterEx.EntityType.Name.Print());

		string[] endpointSegments = filterEx.EndPoint.Split('.');
		MemberExpression propExpression = Expression.Property(param, endpointSegments[0]);
		if (endpointSegments.Length != 1)
		{
			for (int i = 1; i < endpointSegments.Length; i++)
			{
				propExpression = Expression.Property(propExpression, endpointSegments[i]);
			}
		}

		object[] values = filterEx.Value.Split(';');
		switch (filterEx.CompareMethod)
		{
			case CompareMethod.Equals:
				expression = EqualExpression(values, propExpression, filterEx.ExpressionType);
				break;
			case CompareMethod.OriginalContainsInput:
				expression = OriginalContainsInputExpression(values, propExpression, filterEx.ExpressionType);
				break;
			case CompareMethod.ById:
				expression = ByIdExpression(values, propExpression, filterEx.ExpressionType);
				break;
			case CompareMethod.Nested:
				expression = NestedExpression(propExpression, filterEx);
				break;
			default:
				expression = null;
				break;
		}
		if (expression != null)
			expression = expression.InvokeFilterLambda(param, filterEx.EntityType);
		return expression != null;
	}

	/// <summary>
	/// Filter lambda call with spectified type.
	/// </summary>
	/// <param name="param">Parameter expression to create lambda.</param>
	/// <param name="expression">Expression for lambda.</param>
	/// <param name="sourceType">Source data type.</param>
	/// <returns>Lambda expression</returns>
	public static Expression InvokeFilterLambda(this Expression expression, ParameterExpression param, Type sourceType)
	{
		Type yourType = typeof(EntityFrameworkFiltersExtensions);

		MethodInfo methodInfo = yourType.GetMethod(nameof(GetFilterLambda), BindingFlags.NonPublic | BindingFlags.Static);

		MethodInfo genericMethod = methodInfo.MakeGenericMethod(sourceType);

		return (Expression)genericMethod.Invoke(null, [param, expression]);
	}

	/// <summary>
	/// Filter lambda call.
	/// </summary>
	/// <typeparam name="TSource">Source data type.</typeparam>
	/// <param name="param">Parameter expression to create lambda.</param>
	/// <param name="expression">Expression for lambda.</param>
	/// <returns>Lambda expression</returns>
	private static Expression<Func<TSource, bool>> GetFilterLambda<TSource>(ParameterExpression param, Expression expression)
	{
		return Expression.Lambda<Func<TSource, bool>>(expression, param);
	}

	/// <summary>
	/// Creates Equal() lambda expression from array of filter strings
	/// </summary>
	/// <param name="values">Filter strings</param>
	/// <param name="propExpression">A field of property</param>
	/// <param name="expressionType">Type of expression</param>
	/// <returns>Lambda expression with Equal() filter</returns>
	private static Expression EqualExpression
		(object[] values, MemberExpression propExpression, FilterExpressionType expressionType)
	{
		if (values.Length == 0)
			return Expression.Empty();
		Expression expression = Expression.Empty();
		for (int i = 0; i < values.Length; i++)
		{
			if (i == 0)
				expression = GetSingleEqualExpression(values[i], propExpression);
			else
				expression = Expression.OrElse(expression,
					GetSingleEqualExpression(values[i], propExpression));
		}
		if (expressionType == FilterExpressionType.Include)
			return expression;
		else
			return Expression.Not(expression);
	}

	/// <summary>
	/// Creates a binary expression that compares a property to a specified value or range of values.
	/// </summary>
	/// <remarks>This method supports both single-value equality comparisons and range-based comparisons.  For range
	/// comparisons, the input value must be in the format "start..end", where "start"  and "end" are convertible to the
	/// type of the property. If only one bound of the range is  specified, the method generates a single comparison (e.g.,
	/// greater than or equal to "start").</remarks>
	/// <param name="value">The value to compare against. If the value contains a range in the format "start..end",  the method generates a
	/// range comparison. Otherwise, it generates an equality comparison.</param>
	/// <param name="propExpression">The <see cref="MemberExpression"/> representing the property to compare.</param>
	/// <returns>A <see cref="BinaryExpression"/> representing the comparison. If the value specifies a range,  the result is a
	/// conjunction of range comparisons (e.g., greater than or equal to the start  and less than or equal to the end). If
	/// the value specifies a single value, the result is an  equality comparison.</returns>
	/// <exception cref="Exception">Thrown if the value specifies a range but neither the start nor the end of the range can be  converted to the
	/// property's type.</exception>
	private static BinaryExpression GetSingleEqualExpression(object value, MemberExpression propExpression)
	{
		if (value.ToString()!.Contains(".."))
		{
			string valueString = value.ToString();
			object from = ConvertFromObject(valueString.Substring(0, valueString.IndexOf("..")), propExpression.Type);
			object to = ConvertFromObject(valueString.Substring(valueString.IndexOf("..") + 2), propExpression.Type);
			List<BinaryExpression> binaryExpressions = new List<BinaryExpression>();
			if (from != null)
				binaryExpressions.Add(Expression.GreaterThanOrEqual(
					propExpression, GetValueClosure(from, propExpression)));
			if (to != null)
				binaryExpressions.Add(Expression.LessThanOrEqual(
					propExpression, GetValueClosure(to, propExpression)));
			switch (binaryExpressions.Count)
			{
				case 2:
					return Expression.AndAlso(binaryExpressions[0], binaryExpressions[1]);
				case 1:
					return binaryExpressions[0];
				default:
					throw new Exception($"Could not translate expression {valueString}");
			}
		}
		else
		{
			UnaryExpression valueExpression = GetConvertedValueClosure(value, propExpression);
			return Expression.Equal(propExpression, valueExpression);
		}
	}


	/// <summary>
	/// Creates an expression that checks if a property contains any of the specified values, with optional inclusion or
	/// exclusion logic.
	/// </summary>
	/// <param name="values">An array of values to check against the property. Each value is converted to lowercase for comparison.</param>
	/// <param name="propExpression">The property expression to evaluate. This represents the property being checked.</param>
	/// <param name="expressionType">Specifies whether the expression should include or exclude matches.  Use <see cref="FilterExpressionType.Include"/>
	/// to include matches, or <see cref="FilterExpressionType.Exclude"/> to exclude them.</param>
	/// <returns>An <see cref="Expression"/> that evaluates to <see langword="true"/> if the property contains any of the specified
	/// values (or does not contain them, depending on <paramref name="expressionType"/>). Returns an empty expression if
	/// <paramref name="values"/> is empty.</returns>
	private static Expression OriginalContainsInputExpression
		(object[] values, MemberExpression propExpression, FilterExpressionType expressionType)
	{
		if (values.Length == 0)
			return Expression.Empty();

		// Convert property to lower case: x.Property.ToLower()
		var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes);
		var loweredProp = Expression.Call(propExpression, toLowerMethod);

		Expression expression = Expression.Empty();

		for (int i = 0; i < values.Length; i++)
		{
			string valueStr = values[i].ToString()?.ToLowerInvariant() ?? string.Empty;

			// x.Property.ToLower().Contains(value.ToLower())
			var valueExpr = GetValueClosure(valueStr, propExpression);
			var containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)]);
			var containsExpr = Expression.Call(loweredProp, containsMethod, valueExpr);

			if (i == 0)
				expression = containsExpr;
			else
				expression = Expression.OrElse(expression, containsExpr);
		}

		// Invert if Exclude
		return expressionType == FilterExpressionType.Include
			? expression
			: Expression.Not(expression);
	}

	/// <summary>
	/// Creates Equal() lambda expression by id from array of filter strings
	/// </summary>
	/// <param name="values">Filter strings</param>
	/// <param name="propExpression">A field of property</param>
	/// <param name="expressionType">Type of expression</param>
	/// <returns>Lambda expression with Equal() filter by id</returns>
	private static Expression ByIdExpression
		(object[] values, MemberExpression propExpression, FilterExpressionType expressionType)
	{
		string? key = GetForeignKeyFromModel(propExpression.Expression.Type, propExpression.Member.Name);

		if (key != null)
			propExpression = Expression.Property(propExpression.Expression, key);
		else
			throw new NotImplementedException("Not supported operation");

		return EqualExpression(values, propExpression, expressionType);
	}

	/// <summary>
	/// Gets Where statement with inner filter expression.
	/// </summary>
	/// <param name="values">Filter strings.</param>
	/// <param name="propExpression">A field of property.</param>
	/// <param name="filterEx">Filter expression.</param>
	/// <returns>Lambda expression with Where() statement and inner expression.</returns>
	/// <remarks>Output expression example: <code>x.EntitiesList.Where(e => e.Property == value).Count() != 0</code></remarks>
	private static Expression NestedExpression(MemberExpression propExpression, FilterExpression filterEx)
	{
		if (!TryGetLinqExpression(filterEx.InnerFilterExpression, out var filterLambda))
			return null;

		MethodInfo whereMethod = typeof(Enumerable).GetMethods()
			.Where(m => m.Name == "Where")
			.First(m => m.GetParameters().Length == 2)
			.MakeGenericMethod(filterEx.InnerFilterExpression.EntityType);

		// Create Where expressiom
		var whereCallExpression = Expression.Call(
			whereMethod,
			propExpression,
			filterLambda);

		// Get method Count for IEnumerable<T>
		MethodInfo countMethod = typeof(Enumerable).GetMethods()
			.Where(m => m.Name == "Count" && m.GetParameters().Length == 1)
			.Single()
			.MakeGenericMethod(filterEx.InnerFilterExpression.EntityType);

		// Create Count expression
		var countCallExpression = Expression.Call(
			countMethod,
			whereCallExpression);

		// Create expression for comparison with zero 
		var zeroExpression = Expression.Constant(0);
		var notEqualExpression = Expression.NotEqual(countCallExpression, zeroExpression);

		return notEqualExpression;
	}

	private static string? GetForeignKeyFromModel(Type type, string modelName)
	{
		PropertyInfo? property = type.GetProperties().FirstOrDefault(
			p => ((ForeignKeyAttribute)p.GetCustomAttributes(true)
			.FirstOrDefault(a => a.GetType() == typeof(ForeignKeyAttribute)))?.Name == modelName)!;
		if (property == null)
		{
			string idPropertyName = type.GetProperties()
				.FirstOrDefault(p => p.Name == modelName)
				.GetCustomAttribute<ForeignKeyAttribute>()
				.Name;
			property = type.GetProperties()
				.FirstOrDefault(p => p.Name == idPropertyName);
		}
		return property?.Name;
	}

	/// <summary>
	/// Creates a closure expression that encapsulates a value and converts it to the specified type.
	/// </summary>
	/// <remarks>This method is typically used to create an expression tree that captures a value in a closure,
	/// allowing it to be used in dynamic LINQ or other expression-based scenarios.</remarks>
	/// <param name="value">The value to be encapsulated in the closure. Must be convertible to the type of <paramref name="propExpression"/>.</param>
	/// <param name="propExpression">A <see cref="MemberExpression"/> representing the target property or member type for the value conversion.</param>
	/// <returns>A <see cref="UnaryExpression"/> that represents the converted value encapsulated in a closure.</returns>
	private static UnaryExpression GetConvertedValueClosure(object value, MemberExpression propExpression)
	{
		var targetValue = ConvertFromObject(value, propExpression.Type);
		return GetValueClosure(targetValue, propExpression);
	}

	/// <summary>
	/// Creates a closure that encapsulates the specified value.
	/// </summary>
	/// <remarks>This method is typically used to create an expression tree that captures a value in a closure,
	/// allowing it to be used in dynamic LINQ or other expression-based scenarios.</remarks>
	/// <param name="value">The value to be encapsulated in the closure.</param>
	/// <param name="propExpression">The member expression whose type determines the target type of the value conversion.</param>
	/// <returns>A <see cref="UnaryExpression"/> that represents the converted value encapsulated in the closure.</returns>
	private static UnaryExpression GetValueClosure(object value, MemberExpression propExpression)
	{
		ValueHolder valueHolder = new(value);

		var holderExpression = Expression.Constant(valueHolder);
		var valueExpression = Expression.Convert(
			Expression.Property(holderExpression, nameof(ValueHolder.Value)),
			propExpression.Type);
		return valueExpression;
	}

	private static object ConvertFromString(this string value, Type type)
	{
		if (value == "")
			return null;
		if (type == typeof(DateOnly) || type == typeof(DateOnly?))
			return DateOnly.Parse(value);
		return Convert.ChangeType(value, type);
	}

	private static object ConvertFromObject(object value, Type type)
		=> value.ToString().ConvertFromString(type);

	private class ValueHolder
	{
		public object Value { get; set; }

		public ValueHolder(object value)
		{
			Value = value;
		}
	}
}
