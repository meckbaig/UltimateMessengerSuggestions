using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UltimateMessengerSuggestions.Common.Exceptions;
using UltimateMessengerSuggestions.DbContexts;
using UltimateMessengerSuggestions.Models.Db.Enums;
using UltimateMessengerSuggestions.Services.Interfaces;

namespace UltimateMessengerSuggestions.Features.Auth;

/// <summary>
/// Command to check if a user exists in the system based on their messenger id and client name.
/// </summary>
public record GetLoginQuery : IRequest<GetLoginResponse>
{
	/// <summary>
	/// Unique identifier for the user from external messenger.
	/// </summary>
	[FromQuery]
	public required string MessengerId { get; set; }

	/// <summary>
	/// Client name from which the user is registered.
	/// </summary>
	[FromQuery]
	public required string Client { get; set; }
}

/// <summary>
/// Response for the GetLoginCommand indicating that the user exists.
/// </summary>
public class GetLoginResponse
{ 
	/// <summary>
	/// JWT token for the user.
	/// </summary>
	public required string Token { get; set; }
}

internal class GetLoginValidator : AbstractValidator<GetLoginQuery>
{
	public GetLoginValidator()
	{
		RuleFor(x => x.MessengerId)
			.NotEmpty();
		RuleFor(x => x.Client)
			.NotEmpty()
			.Must(Client.IsValid);
	}
}

internal class GetLoginHandler : IRequestHandler<GetLoginQuery, GetLoginResponse>
{
	private readonly IAppDbContext _context;
	private readonly IJwtProvider _jwtProvider;

	public GetLoginHandler(IAppDbContext context, IJwtProvider jwtProvider)
	{
		_context = context;
		_jwtProvider = jwtProvider;
	}

	public async Task<GetLoginResponse> Handle(GetLoginQuery request, CancellationToken cancellationToken)
	{
		var account = await _context.MessengerAccounts
			.FirstOrDefaultAsync(u =>
					u.MessengerId == request.MessengerId &&
					u.Client == request.Client,
				cancellationToken);

		if (account == null)
			throw new EntityNotFoundException("User not found.");

		string token = _jwtProvider.GenerateToken(account);

		return new GetLoginResponse { Token = token };
	}
}
