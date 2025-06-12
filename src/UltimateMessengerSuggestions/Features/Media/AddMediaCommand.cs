using FluentValidation;
using MediatR;
using UltimateMessengerSuggestions.DbContexts;

namespace UltimateMessengerSuggestions.Features.Media;

public record AddMediaCommand : IRequest<AddMediaResponse>
{

}

public class AddMediaResponse
{
}

internal class AddMediaValidator : AbstractValidator<AddMediaCommand>
{
	public AddMediaValidator()
	{
	}
}

internal class AddMediaHandler : IRequestHandler<AddMediaCommand, AddMediaResponse>
{
	private readonly IAppDbContext _context;

	public AddMediaHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<AddMediaResponse> Handle(AddMediaCommand request, CancellationToken cancellationToken)
	{
		return new AddMediaResponse
		{

		};
	}
}
