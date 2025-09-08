using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UltimateMessengerSuggestions.Migrations
{
    /// <inheritdoc />
    public partial class CreateSimpleSearchProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION public.find_media_by_tags_5(__query text, __owner_id integer)
RETURNS TABLE(
    id integer,
    public_id character varying,
    description text,
    media_type text,
    media_url text,
    vk_conversation text,
    vk_message_id bigint,
    discriminator text,
    tag_id integer,
    tag_name text,
    match_count integer
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    WITH words AS (
        SELECT lower(trim(value)) AS word
        FROM unnest(string_to_array(__query, ' ')) AS value
    ),
    matched_tags AS (
        SELECT
            m.id AS media_id,
            t.id AS tag_id,
            t.name AS tag_name
        FROM media_files m
        JOIN media_file_tag mft ON m.id = mft.media_files_id
        JOIN tags t ON t.id = mft.tags_id
        JOIN words w ON lower(t.name) = w.word
        WHERE m.owner_id = __owner_id OR m.is_public = true
    ),
    score_per_media AS (
        SELECT
            mt.media_id,
            COUNT(*)::integer AS match_count
        FROM matched_tags mt
        GROUP BY mt.media_id
    )
    SELECT 
        m.id, m.public_id, m.description, m.media_type, m.media_url,
        v.vk_conversation, v.vk_message_id,
        CASE WHEN v.id IS NOT NULL THEN 'VkVoiceMediaFile' END,
        t.id AS tag_id, t.name AS tag_name,
        s.match_count
    FROM score_per_media s
    JOIN media_files m ON m.id = s.media_id
    LEFT JOIN vk_voice_media_file v ON m.id = v.id
    LEFT JOIN media_file_tag mft ON m.id = mft.media_files_id
    LEFT JOIN tags t ON t.id = mft.tags_id
    WHERE m.owner_id = __owner_id OR m.is_public = true
    ORDER BY s.match_count DESC, m.id;
END;
$$;
			;");
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("DROP FUNCTION find_media_by_tags_5");
		}
    }
}
