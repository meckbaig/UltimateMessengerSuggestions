namespace Meckbaig.Cqrs.ListFliters.Extensions;

public static partial class JsonExtensions
{
	internal static string ToPathFormat(this string property)
	{
		return string.Format("/{0}",
			string.Join(
				'/',
				property
				.Split('.')
				.Select(x => x.Print())));
	}

	internal static string ToPropetyFormat(this string path)
	{
		return string.Join(
			'.',
			path
			.Replace("/", " ")
			.Replace(".", " ")
			.Trim()
			.Split(' ')
			.Select(x => x.ToPascalCase()));
	}
}
