using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UltimateMessengerSuggestions.DbContexts;
using UltimateMessengerSuggestions.Extensions;
using UltimateMessengerSuggestions.Models.Db;
using UltimateMessengerSuggestions.Models.Db.Enums;
using UltimateMessengerSuggestions.Models.Dtos.Features;

namespace UltimateMessengerSuggestions.Features;

/// <summary>
/// Search media by search string query parameters.
/// </summary>
public record GetMediaQuery : IRequest<GetMediaResponse>
{
	/// <summary>
	/// Search string to find media files by tags.
	/// </summary>
	[FromQuery]
	public required string SearchString { get; set; }

	/// <summary>
	/// Client identifier to filter media files by client.
	/// </summary>
	[FromQuery]
	public required string Client { get; set; }
}

/// <summary>
/// Response results with media files that match the search criteria.
/// </summary>
public class GetMediaResponse
{
	/// <summary>
	/// List of media files that match the search criteria.
	/// </summary>
	public required List<MediaFileDto> MediaFiles { get; set; }
}

internal class GetMediaValidator : AbstractValidator<GetMediaQuery>
{
	public GetMediaValidator()
	{
		RuleFor(x => x.SearchString)
			.NotEmpty()
			.MaximumLength(255);
		RuleFor(x => x.Client)
			.NotEmpty()
			.Must(Client.IsValid);
	}

}

internal class GetMediaHandler : IRequestHandler<GetMediaQuery, GetMediaResponse>
{
	private readonly IAppDbContext _context;

	public GetMediaHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<GetMediaResponse> Handle(GetMediaQuery request, CancellationToken cancellationToken)
	{
		var media = await SearchMediaByTags(request.SearchString);

		return new GetMediaResponse
		{
			MediaFiles = media.ToDto().ToList()
		};
	}

	private async Task<List<MediaFile>> SearchMediaByTags(string query)
	{
		var rawPhrases = query
			.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Where(p => p.Length > 1)
			.ToList();

		if (rawPhrases.Count == 0)
			return [];

		// Create copies: full phrases and words
		var phrases = new List<string> { query.Trim().ToLower() }; // full string
		phrases.AddRange(rawPhrases.Where(p => p.Length > 2));     // words as phrases too

		var media = await _context.MediaFiles
			.Select(m => new
			{
				Media = m,
				Relevance =
					m.Tags.Count(t => phrases.Contains(t.Name.ToLower())) * 3 + // exact phrases
					m.Tags.Count(t => phrases.Any(p => EF.Functions.Like(t.Name, "%" + p + "%"))) * 2 + // partial match
					m.Tags.Count(t => rawPhrases.Any(p => EF.Functions.Like(t.Name, "%" + p + "%"))) // by words
			})
			.Where(x => x.Relevance > 0) // exclude irrelevant
			.OrderByDescending(x => x.Relevance)
			.Select(x => x.Media)
			.Include(m => m.Tags)
			.ToListAsync();

		return media;
	}

}
