using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UltimateMessengerSuggestions.Common.Exceptions;
using UltimateMessengerSuggestions.DbContexts;
using UltimateMessengerSuggestions.Models.Db.Enums;

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

	public GetLoginHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<GetLoginResponse> Handle(GetLoginQuery request, CancellationToken cancellationToken)
	{
		bool userExists = await _context.MessengerAccounts
			.AnyAsync(u =>
					u.MessengerId == request.MessengerId &&
					u.Client == request.Client,
				cancellationToken);
		return userExists
			? new GetLoginResponse()
			: throw new EntityNotFoundException("User not found.");
	}
}
