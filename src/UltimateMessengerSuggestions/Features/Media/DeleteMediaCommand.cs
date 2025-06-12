using FluentValidation;
using MediatR;
using UltimateMessengerSuggestions.DbContexts;

namespace UltimateMessengerSuggestions.Features.Media;

public record DeleteMediaCommand : IRequest<DeleteMediaResponse>
{
}

public class DeleteMediaResponse
{
}

internal class DeleteMediaValidator : AbstractValidator<DeleteMediaCommand>
{
	public DeleteMediaValidator()
	{
	}
}

internal class DeleteMediaHandler : IRequestHandler<DeleteMediaCommand, DeleteMediaResponse>
{
	private readonly IAppDbContext _context;

	public DeleteMediaHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<DeleteMediaResponse> Handle(DeleteMediaCommand request, CancellationToken cancellationToken)
	{
		return new DeleteMediaResponse
		{

		};
	}
}
