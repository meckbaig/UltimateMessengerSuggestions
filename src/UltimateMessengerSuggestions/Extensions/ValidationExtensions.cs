using FluentValidation;

namespace UltimateMessengerSuggestions.Extensions;

internal static class ValidationExtensions
{
	public static IRuleBuilderOptions<T, string?> MustBeValidEnum
		<T, TEnum>(this IRuleBuilder<T, string>? ruleBuilder) where TEnum : struct, Enum
	{
		return ruleBuilder.Must((q, p) => BeValidEnum<TEnum>(p))
			.WithMessage((q, p) => $"'{p} is not valid {typeof(TEnum).Name}'");
	}

	private static bool BeValidEnum<TEnum>(string? value) where TEnum : struct, Enum
	{
		return Enum.TryParse<TEnum>(value, true, out var _);
	}
	public static IRuleBuilderOptions<T, string?> MustBeEnum
		<T, TEnum>(this IRuleBuilder<T, string>? ruleBuilder, TEnum enumValue) where TEnum : struct, Enum
	{
		return ruleBuilder.Must((q, p) => BeEnum(p, enumValue))
			.WithMessage((q, p) => $"'{p} is not {enumValue.ToString()}'");
	}

	private static bool BeEnum<TEnum>(string? value, TEnum enumValue) where TEnum : struct, Enum
	{
		return Enum.TryParse<TEnum>(value, true, out var result) && result.Equals(enumValue);
	}
}
