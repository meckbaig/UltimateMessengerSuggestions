using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UltimateMessengerSuggestions.DbContexts;
using UltimateMessengerSuggestions.Extensions;
using UltimateMessengerSuggestions.Models.Db;
using UltimateMessengerSuggestions.Models.Db.Enums;
using UltimateMessengerSuggestions.Models.Dtos.Features.Suggestions;

namespace UltimateMessengerSuggestions.Features.Suggestions;

/// <summary>
/// Search media by search string query parameters.
/// </summary>
public record GetSuggestionsQuery : IRequest<GetSuggestionsResponse>
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
	public string QueryExecutionType { get; set; } = "ef";

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
		/// Entity Framework Core (EF) compiled query execution type.
		/// </summary>
		public const string CompiledEf = "ef-compiled";
	}
}

/// <summary>
/// Response results with media files that match the search criteria.
/// </summary>
public class GetSuggestionsResponse
{
	/// <summary>
	/// List of media files that match the search criteria.
	/// </summary>
	public required List<MediaFileDto> MediaFiles { get; set; }
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
		var media = await SearchMediaByTagsAsync(request.SearchString, request.QueryExecutionType, cancellationToken);

		return new GetSuggestionsResponse
		{
			MediaFiles = media.ToDto().ToList()
		};
	}

	private async Task<List<MediaFile>> SearchMediaByTagsAsync(string query, string execType, CancellationToken cancellationToken)
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
				return await FindByTagsUsingProcedureAsync(fullPhrases, rawWords, cancellationToken);
			case "ef":
				return await FindByTagsUsingEf3Async(fullPhrases, rawWords, cancellationToken);
			case "ef-compiled":
				// return FindByTagsUsingEfCompiledAsync(fullPhrases, rawWords, cancellationToken);
			default:
				throw new NotImplementedException($"Execution type '{execType}' is not implemented.");
		}
	}

	// 60-80ms
	private async Task<List<MediaFile>> FindByTagsUsingEfAsync(IEnumerable<string> fullPhrases, IEnumerable<string> rawWords, CancellationToken cancellationToken)
	{
		return await _context.MediaFiles
			.AsNoTracking()
			.Where(m => m.Tags.Any(t =>
				fullPhrases.Contains(t.Name.ToLower()) ||
				fullPhrases.Any(p => EF.Functions.ILike(t.Name, "%" + p + "%")) ||
				rawWords.Any(w => EF.Functions.ILike(t.Name, "%" + w + "%"))))
			.Select(m => new
			{
				Media = m,
				Relevance =
					m.Tags.Count(t => fullPhrases.Contains(t.Name.ToLower())) * 3 +
					m.Tags.Count(t => fullPhrases.Any(p => EF.Functions.ILike(t.Name, "%" + p + "%"))) * 2 +
					m.Tags.Count(t => rawWords.Any(w => EF.Functions.ILike(t.Name, "%" + w + "%")))
			})
			.Where(x => x.Relevance > 0)
			.OrderByDescending(x => x.Relevance)
			.ThenBy(x => x.Media.Id)
			.Select(x => x.Media)
			.Include(m => m.Tags)
			.ToListAsync(cancellationToken);
	}

	// 50-70ms
	public async Task<List<MediaFile>> FindByTagsUsingEf2Async(IEnumerable<string> fullPhrases, IEnumerable<string> rawWords, CancellationToken cancellationToken)
	{
		// Приводим фразы для точного совпадения в нижний регистр
		var lowerFullPhrases = fullPhrases.Select(fp => fp.ToLower()).ToArray();

		var query = _context.MediaFiles
			.AsNoTracking()
			.Select(mf => new
			{
				MediaFile = mf,
				mf.Tags,
				ExactMatches = mf.Tags.Count(t => lowerFullPhrases.Contains(t.Name.ToLower())) * 3,
				PhraseMatches = mf.Tags.Count(t =>
					!lowerFullPhrases.Contains(t.Name.ToLower()) &&
					fullPhrases.Any(fp => EF.Functions.ILike(t.Name, "%" + fp + "%"))) * 2,
				WordMatches = mf.Tags.Count(t =>
					rawWords.Any(rw => EF.Functions.ILike(t.Name, "%" +rw + "%")))
	
			})
			.Where(x => x.ExactMatches + x.PhraseMatches + x.WordMatches > 0)
			.Select(x => new
			{
				x.MediaFile,
				x.Tags,
				TotalScore = x.ExactMatches + x.PhraseMatches + x.WordMatches
			})
			.OrderByDescending(x => x.TotalScore)
			.ThenBy(x => x.MediaFile.Id);

		// Материализуем запрос
		var results = await query.ToListAsync(cancellationToken);

		// Собираем финальные объекты
		return results.Select(x =>
		{
			// Обрабатываем наследование
			if (x.MediaFile is VkVoiceMediaFile voiceFile)
			{
				voiceFile.Tags = x.Tags.ToList();
				return voiceFile;
			}

			x.MediaFile.Tags = x.Tags.ToList();
			return x.MediaFile;
		}).ToList();
	}

	// 2ms
	public async Task<List<MediaFile>> FindByTagsUsingEf3Async(IEnumerable<string> fullPhrases, IEnumerable<string> rawWords, CancellationToken cancellationToken)
	{
		// Шаг 1: Получаем ID медиафайлов с совпадениями
		var mediaFileIds = await _context.Tags
			.AsNoTracking()
			.Where(t =>
				fullPhrases.Contains(t.Name) ||
				fullPhrases.Any(fp => EF.Functions.ILike(t.Name, "%" + fp + "%")) ||
				rawWords.Any(rw => EF.Functions.ILike(t.Name, "%" + rw + "%")))
			.SelectMany(t => t.MediaFiles.Select(mf => mf.Id))
			.Distinct()
			.ToListAsync(cancellationToken);

		// Шаг 2: Получаем медиафайлы с тегами (полностью в памяти)
		var mediaFiles = _context.MediaFiles
			.AsNoTracking()
			.Where(mf => mediaFileIds.Contains(mf.Id))
			.Include(mf => mf.Tags) // Жадно загружаем теги
			.AsEnumerable() // Переключаемся на вычисления в памяти
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

	//private List<MediaFile> FindByTagsUsingEfCompiledAsync(IEnumerable<string> fullPhrases, IEnumerable<string> rawWords, CancellationToken cancellationToken)
	//{
	//	// Этап 1: Получаем ID через компилированный запрос
	//	var mediaFileIds = GetSuggestionsIdsCompiled((AppDbContext)_context, fullPhrases.ToArray(), rawWords.ToArray());

	//	// Этап 2: Получаем медиафайлы по ID
	//	var mediaFiles = GetSuggestionsFilesByIdsCompiled((AppDbContext)_context, mediaFileIds);

	//	return mediaFiles
	//		.AsEnumerable()
	//		.Select(mf => new
	//		{
	//			MediaFile = mf,
	//			Score = CalculateScore(mf, fullPhrases, rawWords)
	//		})
	//		.Where(x => x.Score > 0)
	//		.OrderByDescending(x => x.Score)
	//		.ThenBy(x => x.MediaFile.Id)
	//		.Select(x => x.MediaFile)
	//		.ToList();
	//}

	//private static readonly Func<AppDbContext, string[], string[], List<int>> GetSuggestionsIdsCompiled =
	//	EF.CompileQuery((AppDbContext context, string[] fullPhrases, string[] rawWords) =>
	//		context.Tags
	//			.AsNoTracking()
	//			.Where(t =>
	//				fullPhrases.Contains(t.Name) ||
	//				rawWords.Any(rw => t.Name.Contains(rw)))
	//			.SelectMany(t => t.MediaFiles.Select(mf => mf.Id))
	//			.Distinct()
	//			.ToList());

	//private static readonly Func<AppDbContext, IEnumerable<int>, List<MediaFile>> GetSuggestionsFilesByIdsCompiled =
	//	EF.CompileQuery((AppDbContext context, IEnumerable<int> ids) =>
	//		context.MediaFiles
	//			.AsNoTracking()
	//			.Include(mf => mf.Tags)
	//			.Where(mf => ids.Contains(mf.Id))
	//			.ToList());

	//private static int CalculateScore(MediaFile mf, IEnumerable<string> fullPhrases, IEnumerable<string> rawWords)
	//{
	//	// Все данные уже в нижнем регистре, поэтому просто Contains
	//	var exactMatches = mf.Tags.Count(t => fullPhrases.Contains(t.Name)) * 3;
	//	var wordMatches = mf.Tags.Count(t => rawWords.Any(rw => t.Name.Contains(rw)));

	//	return exactMatches + wordMatches;
	//}

	// 1ms
	public async Task<List<MediaFile>> FindByTagsUsingProcedureAsync(IEnumerable<string> fullPhrases, IEnumerable<string> rawWords, CancellationToken cancellationToken)
	{
		// procedure call
		var results = await _context.MediaFileSearchResults
			.FromSqlInterpolated($"SELECT * FROM find_media_by_tags({fullPhrases.ToArray()}, {rawWords.ToArray()})")
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
			   mediaFile.Description = first.Description;
			   mediaFile.MediaType = Enum.Parse<MediaType>(first.MediaType, true);
			   mediaFile.MediaUrl = first.MediaUrl;
			   mediaFile.Tags = tags;

			   return mediaFile;
		   })
		   .ToList();
	}
}
