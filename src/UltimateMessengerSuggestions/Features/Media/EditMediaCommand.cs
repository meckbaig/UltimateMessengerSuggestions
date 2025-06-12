using FluentValidation;
using MediatR;
using UltimateMessengerSuggestions.DbContexts;

namespace UltimateMessengerSuggestions.Features.Media;

public record EditMediaCommand : IRequest<EditMediaResponse>
{
}

public class EditMediaResponse
{
}

internal class EditMediaValidator : AbstractValidator<EditMediaCommand>
{
	public EditMediaValidator()
	{
	}
}

internal class EditMediaHandler : IRequestHandler<EditMediaCommand, EditMediaResponse>
{
	private readonly IAppDbContext _context;

	public EditMediaHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<EditMediaResponse> Handle(EditMediaCommand request, CancellationToken cancellationToken)
	{
		return new EditMediaResponse
		{

		};
	}
}
