namespace UltimateMessengerSuggestions.Common.Exceptions;

/// <summary>
/// Exception that returns <see cref="StatusCodes.Status401Unauthorized"/>.
/// </summary>
internal class UnauthorizedException : Exception
{
	public UnauthorizedException(string message) : base(message)
	{
	}
}
