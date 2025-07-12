using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using UltimateMessengerSuggestions.Common.Exceptions;
using UltimateMessengerSuggestions.Common.Options;
using UltimateMessengerSuggestions.Models.Db;
using UltimateMessengerSuggestions.Models.Dtos.Auth;
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
			new(CustomClaim.UserId, account.UserId.ToString()),
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

	public UserLoginDto GetUserLoginFromClaimsPrincipal(ClaimsPrincipal claimsPrincipal)
	{
		return new UserLoginDto(
			Convert.ToInt32(claimsPrincipal.FindFirst(CustomClaim.UserId)?.Value 
				?? throw new UnauthorizedException("User key doesn't have required claims.")),
			claimsPrincipal.FindFirst(CustomClaim.MessengerId)?.Value 
				?? throw new UnauthorizedException("User key doesn't have required claims."),
			claimsPrincipal.FindFirst(CustomClaim.Client)?.Value 
				?? throw new UnauthorizedException("User key doesn't have required claims."));
	}
}

internal static class CustomClaim
{
	public const string UserId = "userId";
	public const string MessengerId = "messengerId";
	public const string Client = "client";
}
