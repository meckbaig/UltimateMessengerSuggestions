using Meckbaig.Cqrs.EntityFrameworkCore.Abstractons.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using UltimateMessengerSuggestions.DbContexts;
using UltimateMessengerSuggestions.Services.Interfaces;

namespace UltimateMessengerSuggestions.Services;

internal class PublicIdGenerator : IPublicIdGenerator
{
	private const int Length = 8;
	private static readonly char[] _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
	private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

	public async Task<string> GenerateUniquePublicIdAsync<TEntity>(IAppDbContext context, CancellationToken cancellationToken = default)
		where TEntity : class, IEntityWithPublicId
	{
		DbContext dbContext = (DbContext)context;
		for (int attempt = 0; attempt < 10; attempt++)
		{
			var id = GenerateRandomId();
			if (!await dbContext.Set<TEntity>().AnyAsync(x => x.PublicId == id, cancellationToken))
				return id;
		}

		throw new Exception("Failed to generate unique PublicId after 10 attempts");
	}

	private string GenerateRandomId()
	{
		var bytes = new byte[Length];
		_rng.GetBytes(bytes);
		var chars = new char[Length];

		for (int i = 0; i < Length; i++)
			chars[i] = _alphabet[bytes[i] % _alphabet.Length];

		return new string(chars);
	}
}
