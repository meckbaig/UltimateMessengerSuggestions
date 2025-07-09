using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using UltimateMessengerSuggestions.Models.Db;
using UltimateMessengerSuggestions.Models.Db.Enums;
using UltimateMessengerSuggestions.Models.Db.ProcedureData;

namespace UltimateMessengerSuggestions.DbContexts;

/// <inheritdoc/>
public class AppDbContext : DbContext, IAppDbContext
{
	/// <inheritdoc/>
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}

	/// <inheritdoc/>
	public DbSet<Tag> Tags => Set<Tag>();

	/// <inheritdoc/>
	public DbSet<MediaFile> MediaFiles => Set<MediaFile>();

	/// <inheritdoc/>
	public DbSet<User> Users => Set<User>();

	/// <inheritdoc/>
	public DbSet<MessengerAccount> MessengerAccounts => Set<MessengerAccount>();

	/// TODO: remove
	public DbSet<MediaFileSearchResult> MediaFileSearchResults { get; set; }


	/// <inheritdoc/>
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<MediaFile>()
			.UseTptMappingStrategy();
		modelBuilder.Entity<VkVoiceMediaFile>()
			.ToTable(ToSnakeCase(nameof(VkVoiceMediaFile)));

		/// TODO: remove
		modelBuilder.Entity<MediaFileSearchResult>().HasNoKey();

		AddTagIndexes(modelBuilder);
		AddMediaTypeConstraint(modelBuilder);

		base.OnModelCreating(modelBuilder);
	}

	private static void AddTagIndexes(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Tag>()
			.HasIndex(b => b.Name)
			.IsUnique();
		modelBuilder.Entity<Tag>()
			.HasIndex(b => b.Name)
			.HasMethod("gin")
			.HasAnnotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
	}

	private static void AddMediaTypeConstraint(ModelBuilder modelBuilder)
	{
		var allowedValues = Enum.GetNames(typeof(MediaType)).Select(e => $"'{ToSnakeCase(e)}'");
		var checkConstraint = $"\"{ToSnakeCase(nameof(MediaFile.MediaType))}\" IN ({string.Join(", ", allowedValues)})";

		modelBuilder.Entity<MediaFile>()
			.Property(m => m.MediaType)
			.HasConversion(
				v => v.ToString().ToLower(),
				v => Enum.Parse<MediaType>(v, true)
			);

		modelBuilder.Entity<MediaFile>()
			.ToTable(t => t.HasCheckConstraint("CK_Message_Type", checkConstraint));
	}

	/// <inheritdoc/>
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseSnakeCaseNamingConvention();
		base.OnConfiguring(optionsBuilder);
	}

	private static string ToSnakeCase(string input)
	{
		return Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2")
					.ToLower();
	}
}
