using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UltimateMessengerSuggestions.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

			migrationBuilder.CreateTable(
				name: "media_files",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					media_type = table.Column<string>(type: "text", nullable: false),
					media_url = table.Column<string>(type: "text", nullable: false),
					description = table.Column<string>(type: "text", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_media_files", x => x.id);
					table.CheckConstraint("CK_Message_Type", "\"media_type\" IN ('voice', 'picture')");
				});

			migrationBuilder.CreateTable(
				name: "tags",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					name = table.Column<string>(type: "text", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_tags", x => x.id);
				});

			migrationBuilder.CreateTable(
				name: "vk_voice_media_file",
				columns: table => new
				{
					id = table.Column<int>(type: "integer", nullable: false),
					vk_conversation = table.Column<string>(type: "text", nullable: false),
					vk_message_id = table.Column<long>(type: "bigint", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_vk_voice_media_file", x => x.id);
					table.ForeignKey(
						name: "fk_vk_voice_media_file_media_files_id",
						column: x => x.id,
						principalTable: "media_files",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "media_file_tag",
				columns: table => new
				{
					media_files_id = table.Column<int>(type: "integer", nullable: false),
					tags_id = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_media_file_tag", x => new { x.media_files_id, x.tags_id });
					table.ForeignKey(
						name: "fk_media_file_tag_media_files_media_files_id",
						column: x => x.media_files_id,
						principalTable: "media_files",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_media_file_tag_tags_tags_id",
						column: x => x.tags_id,
						principalTable: "tags",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "ix_media_file_tag_tags_id",
				table: "media_file_tag",
				column: "tags_id");

			migrationBuilder.CreateIndex(
				name: "ix_tags_name",
				table: "tags",
				column: "name")
				.Annotation("Npgsql:IndexMethod", "gin")
				.Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

			AddQueryProcedure(migrationBuilder);
		}

		private static void AddQueryProcedure(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION public.find_media_by_tags(__fullphrases_0 text[], __rawwords_2 text[])
 RETURNS TABLE(id integer, description text, media_type text, media_url text, vk_conversation text, vk_message_id bigint, discriminator text, tag_id integer, tag_name text)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY 
    WITH phrase_matches AS (
        SELECT
            mft.media_files_id,
            COUNT(*) FILTER (WHERE t.name = ANY(__fullphrases_0)) * 3 AS exact_score,
            COUNT(*) FILTER (WHERE 
                t.name ILIKE ANY(SELECT '%' || f || '%' FROM unnest(__fullphrases_0) f)
            ) * 2 AS phrase_score,
            COUNT(*) FILTER (WHERE 
                t.name ILIKE ANY(SELECT '%' || r || '%' FROM unnest(__rawwords_2) r)
            ) AS word_score
        FROM media_file_tag mft
        INNER JOIN tags t ON mft.tags_id = t.id
        GROUP BY mft.media_files_id
    ),
    media_scores AS (
        SELECT 
            media_files_id,
            (exact_score + phrase_score + word_score) AS total_score
        FROM phrase_matches
        WHERE (exact_score + phrase_score + word_score) > 0
    )
    SELECT 
        m.id, m.description, m.media_type, m.media_url,
        v.vk_conversation, v.vk_message_id,
        CASE WHEN v.id IS NOT NULL THEN 'VkVoiceMediaFile' END,
        t.id, t.name
    FROM media_files m
    LEFT JOIN vk_voice_media_file v ON m.id = v.id
    INNER JOIN media_scores ms ON m.id = ms.media_files_id
    LEFT JOIN media_file_tag mft ON m.id = mft.media_files_id
    LEFT JOIN tags t ON mft.tags_id = t.id
    ORDER BY ms.total_score DESC, m.id;
END;
$function$
			;");
		}



		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "media_file_tag");

            migrationBuilder.DropTable(
                name: "vk_voice_media_file");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "media_files");
        }
    }
}
