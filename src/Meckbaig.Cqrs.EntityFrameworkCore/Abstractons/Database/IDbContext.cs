using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Meckbaig.Cqrs.EntityFrameworkCore.Abstractons.Database;

public interface IDbContext
{
	DatabaseFacade Database { get; }
	DbSet<T> Set<T>() where T : class;
	EntityEntry<T> Entry<T>(T entity) where T : class;
	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
	int SaveChanges();
}
