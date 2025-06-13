using System.Collections;

namespace Meckbaig.Cqrs.Extensions;

/// <summary>
/// Extension methods for <see cref="Type"/> class.
/// </summary>
public static class TypeExtensions
{
	/// <summary>
	/// Determines whether the specified <see cref="Type"/> represents a collection type.
	/// </summary>
	/// <param name="type">The <see cref="Type"/> to evaluate. Must not be <see langword="null"/>.</param>
	/// <returns><see langword="true"/> if the specified <see cref="Type"/> implements <see cref="IEnumerable"/>  
	/// and is not a <see cref="string"/>; otherwise, <see langword="false"/>.</returns>
	public static bool IsCollection(this Type type)
	{
		if (type != typeof(string))
		{
			return typeof(IEnumerable).IsAssignableFrom(type);
		}

		return false;
	}
}
