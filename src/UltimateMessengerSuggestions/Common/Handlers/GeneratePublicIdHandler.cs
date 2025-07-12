using Meckbaig.Cqrs.EntityFrameworkCore.Abstractons.Entities;
using Microsoft.EntityFrameworkCore;
using UltimateMessengerSuggestions.Common.Handlers.Interfaces;
using UltimateMessengerSuggestions.DbContexts;
using UltimateMessengerSuggestions.Services.Interfaces;

namespace UltimateMessengerSuggestions.Common.Handlers;

internal class GeneratePublicIdHandler : IGeneratePublicIdHandler
{
	private readonly IPublicIdGenerator _generator;

	public GeneratePublicIdHandler(IPublicIdGenerator generator)
	{
		_generator = generator;
	}

	public async Task AssignPublicIdsAsync<TContext>(TContext context, CancellationToken cancellationToken) where TContext : DbContext, IAppDbContext
	{
		var entries = context.ChangeTracker.Entries<IEntityWithPublicId>()
			.Where(e => e.State == EntityState.Added && string.IsNullOrEmpty(e.Entity.PublicId));

		foreach (var entry in entries)
		{
			var entityType = entry.Entity.GetType();

			var method = typeof(IPublicIdGenerator)
				.GetMethod(nameof(IPublicIdGenerator.GenerateUniquePublicIdAsync))!
				.MakeGenericMethod(entityType);

			var task = (Task<string>)method.Invoke(_generator, [context, cancellationToken])!;
			entry.Entity.PublicId = await task;
		}
	}
}
