using System.Runtime.CompilerServices;

namespace Meckbaig.Cqrs.ListFliters.Attrubutes;

public class FilterableAttribute : Attribute
{
	public CompareMethod CompareMethod { get; set; }
	public string Path { get; set; }

	public FilterableAttribute(CompareMethod compareMethod, [CallerMemberName] string path = "")
	{
		CompareMethod = compareMethod;
		Path = path;
	}
}

/// <summary>
/// Enumeration for comparing methods used in filtering expressions.
/// </summary>
/// <remarks>
/// <code>
/// Equals - 100% equality
/// ById - entity by id (foreign key)
/// Nested - collection of entities
/// </code>
/// </remarks>
public enum CompareMethod
{
	Undefined = -1,

	/// <summary>
	/// 100% equality
	/// </summary>
	Equals,

	/// <summary>
	/// Determines whether the original string contains the specified input string.
	/// </summary>
	/// <remarks>This method performs a case-insensitive search to determine if the input string is present within the
	/// original string.</remarks>
	OriginalContainsInput,

	/// <summary>
	/// Entity by Id (foreign key)
	/// </summary>
	ById,

	/// <summary>
	/// Collection of entities (nested)
	/// </summary>
	Nested,

	/// <summary>
	/// Custom comparison method, used for more complex filtering scenarios where the default methods do not suffice.
	/// </summary>
	/// <remarks>
	/// This method requeires a custom implementation to define how the comparison should be performed.
	/// </remarks>
	Custom
}
