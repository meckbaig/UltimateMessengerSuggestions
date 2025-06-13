using AutoMapper;
using AutoMapper.Internal;
using Meckbaig.Cqrs.Dto.Abstractions;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Meckbaig.Cqrs.Dto.Extensions;

public static class DtoExtension
{
	/// <summary>
	/// Gets map source name from DTO property name.
	/// </summary>
	/// <param name="dtoProperty">DTO property name.</param>
	/// <param name="provider">Configuraion provider for performing maps.</param>
	/// <param name="nextPropertyType">The type of the DTO containing the parameter. Replaced by the parameter type.</param>
	/// <param name="sourceProperty">Source property path.</param>
	/// <param name="throwException">Throws exception if <see langword="true"/>; otherwise, returns <see langword="false"/>.</param>
	/// <returns><see langword="true"/> if <paramref name="sourceProperty" /> was found successfully; otherwise, <see langword="false"/>.</returns>
	public static bool InvokeTryGetSource(
		string dtoProperty,
		IConfigurationProvider provider,
		ref Type nextPropertyType,
		out string sourceProperty,
		out string errorMessage,
		bool throwException = true)
	{
		var methodInfo = typeof(DtoExtension).GetMethod(
							nameof(TryGetSource),
							BindingFlags.Static | BindingFlags.NonPublic);
		var genericMethod = methodInfo.MakeGenericMethod(GetDtoOriginType(nextPropertyType), nextPropertyType);
		object[] parameters = [dtoProperty, provider, null, null, null, throwException];
		object result = genericMethod.Invoke(null, parameters);
		bool boolResult = (bool)result;
		if (boolResult)
		{
			sourceProperty = (string)parameters[2];
			nextPropertyType = (Type)parameters[3];
		}
		else
		{
			sourceProperty = null;
		}
		errorMessage = (string)parameters[4];
		return boolResult;
	}

	/// <summary>
	/// Gets map source name from DTO property name.
	/// </summary>
	/// <typeparam name="TSource">Source of DTO type.</typeparam>
	/// <typeparam name="TDto">DTO type.</typeparam>
	/// <param name="provider">Configuraion provider for performing maps.</param>
	/// <param name="dtoProperty">DTO property name.</param>
	/// <returns>Source property path.</returns>
	public static string GetSource<TSource, TDto>(
		string dtoProperty,
		IConfigurationProvider provider,
		bool throwException = true)
	{
		if (TryGetSource<TSource, TDto>(
			dtoProperty,
			provider,
			out string source,
			out Type _,
			out string _,
			throwException))
		{
			return source;
		}
		return null;
	}

	/// <summary>
	/// Gets map source name from DTO property name.
	/// </summary>
	/// <typeparam name="TSource">Source of DTO type.</typeparam>
	/// <typeparam name="TDto">DTO type.</typeparam>
	/// <param name="dtoProperty">DTO property name.</param>
	/// <param name="provider">Configuraion provider for performing maps.</param>
	/// <param name="sourceProperty">Source property name.</param>
	/// <param name="dtoPropertyType">DTO property type.</param>
	/// <param name="errorMessage">Message if error occures; otherwise, <see langword="null"/>.</param>
	/// <param name="throwException">Throws exception if <see langword="true"/>; otherwise, returns false.</param>
	/// <returns><see langword="true"/> if <paramref name="dtoPropertyType" /> was found successfully; otherwise, <see langword="false"/>.</returns>
	private static bool TryGetSource<TSource, TDto>(
		string dtoProperty,
		IConfigurationProvider provider,
		out string sourceProperty,
		out Type dtoPropertyType,
		out string errorMessage,
		bool throwException = true)
	{
		if (typeof(IEditDto).IsAssignableFrom(typeof(TDto)))
			return TryGetEditSource<TSource, TDto>(
				dtoProperty,
				provider,
				out sourceProperty,
				out dtoPropertyType,
				out errorMessage,
				throwException);

		errorMessage = null;
		var internalApi = provider.Internal();
		var map = internalApi.FindTypeMapFor<TSource, TDto>();

		if (map == null)
			return GetSourceError($"Property mapping for type of '{dtoProperty.Print()}' does not exist",
				dtoProperty, out sourceProperty, out dtoPropertyType, out errorMessage, throwException);

		var propertyMap = map.PropertyMaps.FirstOrDefault(pm => pm.DestinationMember.Name == dtoProperty);

		if (propertyMap == null)
			return GetSourceError($"Property '{dtoProperty.Print()}' does not exist",
				dtoProperty, out sourceProperty, out dtoPropertyType, out errorMessage, throwException);

		dtoPropertyType = propertyMap?.DestinationType;
		if (propertyMap?.SourceMember?.Name != null)
		{
			sourceProperty = propertyMap?.SourceMember?.Name;
			return true;
		}
		sourceProperty = GetPropertyMapSource(propertyMap.CustomMapExpression.Body);
		return true;
	}

	/// <summary>
	/// Gets map source name from DTO property name but in a reverse.
	/// </summary>
	private static bool TryGetEditSource<TSource, TDto>(
		string dtoProperty,
		IConfigurationProvider provider,
		out string sourceProperty,
		out Type dtoPropertyType,
		out string errorMessage,
		bool throwException)
	{
		errorMessage = null;
		var internalApi = provider.Internal();
		var map = internalApi.FindTypeMapFor<TDto, TSource>();

		if (map == null)
			return GetSourceError($"Property mapping for type of '{dtoProperty.Print()}' does not exist",
				dtoProperty, out sourceProperty, out dtoPropertyType, out errorMessage, throwException);

		MemberMap propertyMap = map.PropertyMaps.FirstOrDefault(pm => pm.SourceMember?.Name == dtoProperty);

		if (propertyMap == null)
		{
			var sdfsd = map.PropertyMaps.Select(pm => pm.CustomMapExpression?.ToString().Split('(', ')', '.', '+')).ToList();
			propertyMap = map.PropertyMaps.FirstOrDefault(
				pm => pm.CustomMapExpression != null &&
				pm.CustomMapExpression.ToString().Split('(', ')', '.', '+').Contains(dtoProperty));
		}
		if (propertyMap == null)
		{
			propertyMap = map.PropertyMaps
				.FirstOrDefault(pm => pm.TypeMap.PathMaps.Any(pm => pm.SourceMember?.Name == dtoProperty))?
				.TypeMap?
				.PathMaps?
				.FirstOrDefault(pm => pm.SourceMember?.Name == dtoProperty);
		}
		if (propertyMap == null)
		{
			return GetSourceError($"Property '{dtoProperty.Print()}' does not exist",
				dtoProperty, out sourceProperty, out dtoPropertyType, out errorMessage, throwException);
		}

		dtoPropertyType = propertyMap?.SourceType;
		if (propertyMap?.DestinationName != null)
		{
			sourceProperty = propertyMap?.DestinationName;
			return true;
		}
		sourceProperty = null;
		errorMessage = "Not supported get source action";
		return false;
	}

	/// <summary>
	/// Gets origin of provided DTO type.
	/// </summary>
	/// <param name="dtoType">DTO type.</param>
	/// <returns>DTO source type.</returns>
	public static Type GetDtoOriginType(Type dtoType)
	{
		if (!typeof(IBaseDto).IsAssignableFrom(dtoType))
			throw new ArgumentException($"{dtoType.Name} does not implement the interface {nameof(IBaseDto)}");

		MethodInfo? method = null;
		Type? currentType = dtoType;

		while (currentType != null && method == null)
		{
			method = currentType.GetMethod(nameof(IBaseDto.GetOriginType),
				BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

			currentType = currentType.BaseType;
		}

		if (method == null)
			throw new InvalidOperationException($"Static method '{nameof(IBaseDto.GetOriginType)}' not found in {dtoType.FullName} or its base types.");

		var result = method.Invoke(null, null);
		return (Type)result;
	}

	public static Type GetDtoValidatorType(Type dtoType)
	{
		if (!typeof(IEditDto).IsAssignableFrom(dtoType))
			throw new ArgumentException($"{dtoType.Name} does not implement the interface {nameof(IEditDto)}");

		MethodInfo method = dtoType.GetMethod(nameof(IEditDto.GetValidatorType), BindingFlags.Static | BindingFlags.Public);
		var result = method.Invoke(null, null);
		return (Type)result;
	}










	/// <summary>
	/// Get map source name from AutoMapper custom lambda function
	/// </summary>
	/// <param name="body">AutoMapper custom lambda function</param>
	/// <returns>Map source string</returns>
	/// <remarks>
	/// Helps with getting DTO map source (to add filters to source)
	/// </remarks>
	private static string GetPropertyMapSource(Expression body)
	{
		Regex regex = new Regex("[^a-zA-Z0-9.]");
		return regex.Replace(body.ToString().Substring(body.ToString().IndexOf('.') + 1), "");
	}

	private static bool GetSourceError(string newErrorMessage, string dtoProperty, out string sourceProperty, out Type dtoPropertyType, out string errorMessage, bool throwException)
	{
		errorMessage = newErrorMessage;
		if (!throwException)
		{
			sourceProperty = null!;
			dtoPropertyType = null!;
			return false;
		}
		else
		{
			throw new ArgumentException(errorMessage);
		}
	}
}
