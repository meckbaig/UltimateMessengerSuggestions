using Microsoft.EntityFrameworkCore;
using UltimateMessengerSuggestions.DbContexts;

namespace UltimateMessengerSuggestions.Common.Handlers.Interfaces;

/// <summary>
/// Class for handling public ID generation for entities in the database context.
/// </summary>
internal interface IGeneratePublicIdHandler
{
	/// <summary>
	/// Asynchronously assigns public IDs to entities in the database context.
	/// </summary>
	/// <param name="context">Databse context.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task AssignPublicIdsAsync<TContext>(TContext context, CancellationToken cancellationToken) where TContext : DbContext, IAppDbContext;
}
