using Microsoft.EntityFrameworkCore;
using UltimateMessengerSuggestions.Models.Db;
using UltimateMessengerSuggestions.Models.Db.ProcedureData;

namespace UltimateMessengerSuggestions.DbContexts;

internal interface IAppDbContext
{
	DbSet<Tag> Tags { get; }
	DbSet<MediaFile> MediaFiles { get; }
	DbSet<User> Users { get; }
	DbSet<MessengerAccount> MessengerAccounts { get; }
	DbSet<MediaFileSearchResult> MediaFileSearchResults { get; }

	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
