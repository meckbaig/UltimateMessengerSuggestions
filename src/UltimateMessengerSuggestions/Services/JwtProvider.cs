using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using UltimateMessengerSuggestions.Common.Options;
using UltimateMessengerSuggestions.Models.Db;
using UltimateMessengerSuggestions.Services.Interfaces;

namespace UltimateMessengerSuggestions.Services;

internal class JwtProvider : IJwtProvider
{
	private readonly JwtOptions _options;

	public JwtProvider(IOptions<JwtOptions> options)
	{
		_options = options.Value;
	}

	public string GenerateToken(MessengerAccount account)
	{
		var tokenHandler = new JsonWebTokenHandler();
		var claims = new List<Claim>
		{
			new(CustomClaim.MessengerId, account.MessengerId),
			new(CustomClaim.Client, account.Client),
		};

		var tokenDesctiptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(claims),
			Expires = null,
			Issuer = _options.Issuer,
			Audience = _options.Audience,
			SigningCredentials = new SigningCredentials(
				new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
				SecurityAlgorithms.HmacSha256)
		};

		return tokenHandler.CreateToken(tokenDesctiptor);
	}
}

internal static class CustomClaim
{
	public const string MessengerId = "messengerId";
	public const string Client = "client";
}
