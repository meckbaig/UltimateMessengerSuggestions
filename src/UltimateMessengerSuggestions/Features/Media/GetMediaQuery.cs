using AutoMapper;
using Meckbaig.Cqrs.ListFliters.Abstractions;
using Meckbaig.Cqrs.ListFliters.Extensions;
using Meckbaig.Cqrs.ListFliters.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using UltimateMessengerSuggestions.DbContexts;
using UltimateMessengerSuggestions.Extensions;
using UltimateMessengerSuggestions.Models.Db;
using UltimateMessengerSuggestions.Models.Dtos.Features.Media;

namespace UltimateMessengerSuggestions.Features.Media;

/// <summary>
/// Query to retrieve a list of media files with filtering and pagination options.
/// </summary>
public record GetMediaQuery : BaseListQuery<GetMediaResponse>
{
	/// <inheritdoc />
	[FromQuery]
	public override int Skip { get; init; }
	/// <inheritdoc />
	[FromQuery]
	public override int Take { get; init; }
	/// <inheritdoc />
	[FromQuery]
	public override string[]? Filters { get; init; }
	/// <inheritdoc />
	[FromQuery]
	public override string[]? OrderBy { get; init; }
}

/// <summary>
/// Response for the GetMediaQuery, containing a list of media files.
/// </summary>
public class GetMediaResponse : BaseListQueryResponse<MediaFileDto>
{

}

internal class GetMediaQueryValidator : BaseListQueryValidator
	<GetMediaQuery, GetMediaResponse, MediaFileDto, MediaFile>
{
	public GetMediaQueryValidator(IMapper mapper) : base(mapper)
	{

	}
}

internal class GetMediaQueryHandler : IRequestHandler<GetMediaQuery, GetMediaResponse>
{
	private readonly IAppDbContext _context;

	public GetMediaQueryHandler(IAppDbContext context)
	{
		_context = context;
	}

	public async Task<GetMediaResponse> Handle(GetMediaQuery request, CancellationToken cancellationToken)
	{
		request.AddFilterExpression(nameof(MediaFileDto.Tags), GetTagsFilterExpression);

		var mediaFiles = await _context.MediaFiles.Include(m => m.Tags)
			.AddFilters(request.GetFilterExpressions())
			.AddOrderBy(request.GetOrderExpressions())
			.Skip(request.Skip).Take(request.Take > 0 ? request.Take : int.MaxValue)
			.ToListAsync(cancellationToken);

		var result = mediaFiles.ToDtos();

		return new GetMediaResponse
		{
			Items = result.ToList(),
		};
	}

	private Expression<Func<MediaFile, bool>> GetTagsFilterExpression(FilterExpression? filterEx)
	{
		if (filterEx is null || string.IsNullOrWhiteSpace(filterEx.Value))
			return _ => true;

		var loweredValue = new string(
			filterEx.Value
			.Trim()
			.ToLowerInvariant()
			.Where(c => !char.IsPunctuation(c))
			.ToArray());

		return filterEx.ExpressionType switch
		{
			FilterExpressionType.Include => mediaFile =>
				mediaFile.Tags.Any(tag => tag.Name.Contains(loweredValue)),

			FilterExpressionType.Exclude => mediaFile =>
				mediaFile.Tags.All(tag => !tag.Name.Contains(loweredValue)),

			_ => _ => true
		};
	}
}
