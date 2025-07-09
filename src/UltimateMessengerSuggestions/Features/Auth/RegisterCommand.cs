using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;
using UltimateMessengerSuggestions.Common.Exceptions;
using UltimateMessengerSuggestions.DbContexts;
using UltimateMessengerSuggestions.Models.Db;
using UltimateMessengerSuggestions.Models.Db.Enums;
using UltimateMessengerSuggestions.Services.Interfaces;
using static UltimateMessengerSuggestions.Features.Auth.RegisterCommand;

namespace UltimateMessengerSuggestions.Features.Auth;

/// <summary>
/// Command to register a new user in the system.
/// </summary>
public record RegisterCommand : IRequest<RegisterResponse>
{
	/// <summary>
	/// The body of the request containing the user registration information.
	/// </summary>
	[FromBody]
	public required BodyParameters Body { get; init; }

	/// <summary>
	/// Parameters for the body of the request to register a user.
	/// </summary>
	public record BodyParameters
	{
		/// <summary>
		/// Hash of the user, used for authentication and identification.
		/// </summary>
		public required string UserHash { get; set; }

		/// <summary>
		/// Unique identifier for the user from external messenger.
		/// </summary>
		public required string MessengerId { get; set; }

		/// <summary>
		/// Client name from which the user is registering.
		/// </summary>
		public required string Client { get; set; }
	}
}

/// <summary>
/// Response for the RegisterCommand indicating the success of the registration operation.
/// </summary>
public class RegisterResponse
{
	/// <summary>
	/// JWT token for the user.
	/// </summary>
	public required string Token { get; set; }
}

internal class RegisterValidator : AbstractValidator<RegisterCommand>
{
	public RegisterValidator()
	{
		RuleFor(x => x.Body)
			.NotNull()
			.SetValidator(new BodyParametersValidator());
	}

	internal class BodyParametersValidator : AbstractValidator<BodyParameters>
	{
		public BodyParametersValidator()
		{
			RuleFor(x => x.UserHash)
				.NotEmpty();
			RuleFor(x => x.MessengerId)
				.NotEmpty();
			RuleFor(x => x.Client)
				.NotEmpty()
				.Must(Client.IsValid);
		}
	}
}

internal class RegisterHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
	private readonly IAppDbContext _context;
	private readonly IJwtProvider _jwtProvider;

	public RegisterHandler(IAppDbContext context, IJwtProvider jwtProvider)
	{
		_context = context;
		_jwtProvider = jwtProvider;
	}

	public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
	{
		var user = await _context.Users
			.Include(u => u.MessengerAccounts)
			.FirstOrDefaultAsync(u => u.UserHash == request.Body.UserHash, cancellationToken: cancellationToken);
		if (user == null)
		{
			throw new EntityNotFoundException($"User with hash {request.Body.UserHash} not found.");
		}

		var account = user.MessengerAccounts
			.SingleOrDefault(ma =>
				ma.MessengerId == request.Body.MessengerId &&
				ma.Client == request.Body.Client);

		if (account == null)
		{
			account = new MessengerAccount
			{
				MessengerId = request.Body.MessengerId,
				Client = request.Body.Client,
				User = user
			};
			user.MessengerAccounts.Add(account);
			await _context.SaveChangesAsync(cancellationToken);
		}

		string token = _jwtProvider.GenerateToken(account);

		return new RegisterResponse { Token = token };
	}
}
