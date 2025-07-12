using Meckbaig.Cqrs.EntityFrameworkCore.Abstractons.Entities;
using UltimateMessengerSuggestions.DbContexts;

namespace UltimateMessengerSuggestions.Services.Interfaces;

/// <summary>
/// Service for generating public IDs for media files.
/// </summary>
internal interface IPublicIdGenerator
{
	/// <summary>
	/// Generates a public ID for a media file.
	/// </summary>
	/// <param name="context">Database context.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A unique public ID.</returns>
	Task<string> GenerateUniquePublicIdAsync<TEntity>(IAppDbContext context, CancellationToken cancellationToken = default) where TEntity : class, IEntityWithPublicId;
}
