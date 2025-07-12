using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UltimateMessengerSuggestions.Migrations
{
    /// <inheritdoc />
    public partial class AlterFindMediaByTagsProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			AlterQueryProcedure(migrationBuilder);
		}

		private static void AlterQueryProcedure(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION public.find_media_by_tags(__fullphrases_0 text[], __rawwords_2 text[], __owner_id int4)
 RETURNS TABLE(id integer, public_id varchar(8), description text, media_type text, media_url text, vk_conversation text, vk_message_id bigint, discriminator text, tag_id integer, tag_name text)
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
        m.id, m.public_id, m.description, m.media_type, m.media_url,
        v.vk_conversation, v.vk_message_id,
        CASE WHEN v.id IS NOT NULL THEN 'VkVoiceMediaFile' END,
        t.id, t.name
    FROM media_files m
    LEFT JOIN vk_voice_media_file v ON m.id = v.id
    INNER JOIN media_scores ms ON m.id = ms.media_files_id
    LEFT JOIN media_file_tag mft ON m.id = mft.media_files_id
    LEFT JOIN tags t ON mft.tags_id = t.id
    WHERE m.owner_id = __owner_id OR m.is_public = true
    ORDER BY ms.total_score DESC, m.id;
END;
$function$
			;");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
