using Microsoft.EntityFrameworkCore;
using UltimateMessengerSuggestions.Models.Db;

namespace UltimateMessengerSuggestions.DbContexts;

internal interface IAppDbContext
{
	DbSet<Tag> Tags { get; }
	DbSet<MediaFile> MediaFiles { get; }

	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
