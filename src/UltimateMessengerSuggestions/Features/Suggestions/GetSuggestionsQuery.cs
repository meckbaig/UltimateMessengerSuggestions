using FluentValidation;
using Meckbaig.Cqrs.Abstractons;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UltimateMessengerSuggestions.Common.Abstractions;
using UltimateMessengerSuggestions.Common.Exceptions;
using UltimateMessengerSuggestions.DbContexts;
using UltimateMessengerSuggestions.Extensions;
using UltimateMessengerSuggestions.Models.Db;
using UltimateMessengerSuggestions.Models.Db.Enums;
using UltimateMessengerSuggestions.Models.Dtos.Auth;
using UltimateMessengerSuggestions.Models.Dtos.Features.Media;

namespace UltimateMessengerSuggestions.Features.Suggestions;

/// <summary>
/// Search media by search string query parameters.
/// </summary>
public record GetSuggestionsQuery : BaseAuthentificatedRequest<UserLoginDto, GetSuggestionsResponse>
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

	/// <summary>
	/// Execution type for the query.
	/// </summary>
	[FromQuery]
	public string QueryExecutionType { get; set; } = ExecutionType.ProcedureSimple;

	/// <summary>
	/// Execution types for the query.
	/// </summary>
	public static class ExecutionType
	{
		/// <summary>
		/// Entity Framework Core (EF) execution type.
		/// </summary>
		public const string Ef = "ef";

		/// <summary>
		/// Stored procedure execution type.
		/// </summary>
		public const string Procedure = "procedure";

		/// <summary>
		/// Stored procedure execution type (with strong similarity and 1/3 desc).
		/// </summary>
		public const string ProcedureSimple = "procedure-simple";
	}
}

/// <summary>
/// Response results with media files that match the search criteria.
/// </summary>
public class GetSuggestionsResponse: BaseResponse
{
	/// <summary>
	/// List of media files that match the search criteria.
	/// </summary>
	public required List<MediaFileDto> Items { get; set; }
}

internal class GetSuggestionsValidator : AbstractValidator<GetSuggestionsQuery>
{
	public GetSuggestionsValidator()
	{
		RuleFor(x => x.SearchString)
			.NotEmpty()
			.MaximumLength(255);
		RuleFor(x => x.Client)
			.NotEmpty()
			.Must(Client.IsValid);
	}
}

internal class GetSuggestionsHandler : IRequestHandler<GetSuggestionsQuery, GetSuggestionsResponse>
{
	private readonly IAppDbContext _context;

	public GetSuggestionsHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<GetSuggestionsResponse> Handle(GetSuggestionsQuery request, CancellationToken cancellationToken)
	{
		if (!request.IsAuthenticated)
			throw new UnauthorizedException("User is not authenticated.");

		var media = await SearchMediaByTagsAsync(request.SearchString, request.QueryExecutionType, request.UserLogin.UserId, cancellationToken);

		return new GetSuggestionsResponse
		{
			Items = media.ToDtos().ToList()
		};
	}

	private async Task<List<MediaFile>> SearchMediaByTagsAsync(string query, string execType, int userId, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(query))
			return [];

		var loweredQuery = new string(query.Trim().ToLowerInvariant().Where(c => !char.IsPunctuation(c)).ToArray());

		var fullPhrases = new List<string> { loweredQuery };
		var rawWords = loweredQuery
			.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Where(p => p.Length > 2)
			.Distinct()
			.ToList();

		/// TODO: remove
		switch (execType)
		{
			case "procedure":
				return await FindByTagsUsingProcedureAsync(fullPhrases, rawWords, userId, cancellationToken);
			case "procedure-simple":
				return await FindByTagsUsingSimpleProcedureAsync(loweredQuery, userId, cancellationToken);
			case "ef":
				return await FindByTagsUsingEf3Async(fullPhrases, rawWords, userId, cancellationToken);
			default:
				throw new NotImplementedException($"Execution type '{execType}' is not implemented.");
		}
	}

	// 2ms
	public async Task<List<MediaFile>> FindByTagsUsingEf3Async(
		IEnumerable<string> fullPhrases,
		IEnumerable<string> rawWords,
		int userId,
		CancellationToken cancellationToken)
	{
		// Step 1: Get media file IDs with matches
		var mediaFileIds = await _context.Tags
			.AsNoTracking()
			.Where(t =>
				fullPhrases.Contains(t.Name) ||
				fullPhrases.Any(fp => EF.Functions.ILike(t.Name, "%" + fp + "%")) ||
				rawWords.Any(rw => EF.Functions.ILike(t.Name, "%" + rw + "%")))
			.SelectMany(t => t.MediaFiles.Select(mf => mf.Id))
			.Distinct()
			.ToListAsync(cancellationToken);

		// Step 2: Get media files by IDs and calculate scores (in memory)
		var mediaFiles = _context.MediaFiles
			.AsNoTracking()
			.Where(mf => 
				mediaFileIds.Contains(mf.Id) && 
				(mf.OwnerId == userId || mf.IsPublic))
			.Include(mf => mf.Tags)
			.AsEnumerable() // Materialize the query to memory
			.Select(mf => new
			{
				MediaFile = mf,
				ExactMatches = mf.Tags.Count(t => fullPhrases.Contains(t.Name)) * 3,
				PhraseMatches = mf.Tags.Count(t =>
					!fullPhrases.Contains(t.Name) &&
					fullPhrases.Any(fp => t.Name.Contains(fp))) * 2,
				WordMatches = mf.Tags.Count(t =>
					rawWords.Any(rw => t.Name.Contains(rw)))
			})
			.Where(x => x.ExactMatches + x.PhraseMatches + x.WordMatches > 0)
			.OrderByDescending(x => x.ExactMatches + x.PhraseMatches + x.WordMatches)
			.ThenBy(x => x.MediaFile.Id)
			.Select(x => x.MediaFile)
			.ToList();

		return mediaFiles;
	}

	// 1ms
	public async Task<List<MediaFile>> FindByTagsUsingProcedureAsync(
		IEnumerable<string> fullPhrases,
		IEnumerable<string> rawWords,
		int userId,
		CancellationToken cancellationToken)
	{
		// procedure call
		var results = await _context.MediaFileSearchResults
			.FromSqlInterpolated($"SELECT * FROM find_media_by_tags({fullPhrases.ToArray()}, {rawWords.ToArray()}, {userId})")
			.ToListAsync(cancellationToken);

		// data grouping and transformation
		return results
			.GroupBy(r => r.Id)
			.Select(g =>
			{
				var first = g.First();
				var tags = g
					.Where(x => x.TagId.HasValue)
					.Select(x => new Tag { Id = x.TagId.Value, Name = x.TagName })
					.ToList();

				MediaFile mediaFile = first.Discriminator == "VkVoiceMediaFile"
					? new VkVoiceMediaFile
					{
						VkConversation = first.VkConversation,
						VkMessageId = first.VkMessageId.Value
					}
					: new MediaFile();

				mediaFile.Id = first.Id;
				mediaFile.PublicId = first.PublicId;
				mediaFile.Description = first.Description;
				mediaFile.MediaType = Enum.Parse<MediaType>(first.MediaType, true);
				mediaFile.MediaUrl = first.MediaUrl;
				mediaFile.Tags = tags;

				return mediaFile;
			})
			.ToList();
	}

	// 1ms
	public async Task<List<MediaFile>> FindByTagsUsingSimpleProcedureAsync(
		string fullPhrase,
		int userId,
		CancellationToken cancellationToken)
	{
		// procedure call
		var results = await _context.MediaFileSearchResults
			.FromSqlInterpolated($"SELECT * FROM find_media_by_tags_7({fullPhrase}, {userId})")
			.ToListAsync(cancellationToken);

		// data grouping and transformation
		return results
			.GroupBy(r => r.Id)
			.Select(g =>
			{
				var first = g.First();
				var tags = g
					.Where(x => x.TagId.HasValue)
					.Select(x => new Tag { Id = x.TagId.Value, Name = x.TagName })
					.ToList();

				MediaFile mediaFile = first.Discriminator == "VkVoiceMediaFile"
					? new VkVoiceMediaFile
					{
						VkConversation = first.VkConversation,
						VkMessageId = first.VkMessageId.Value
					}
					: new MediaFile();

				mediaFile.Id = first.Id;
				mediaFile.PublicId = first.PublicId;
				mediaFile.Description = first.Description;
				mediaFile.MediaType = Enum.Parse<MediaType>(first.MediaType, true);
				mediaFile.MediaUrl = first.MediaUrl;
				mediaFile.Tags = tags;

				return mediaFile;
			})
			.ToList();
	}
}
