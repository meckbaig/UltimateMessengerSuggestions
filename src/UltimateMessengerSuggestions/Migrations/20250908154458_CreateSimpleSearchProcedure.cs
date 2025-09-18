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
CREATE OR REPLACE FUNCTION public.find_media_by_tags_7(__query text, __owner_id integer)
 RETURNS TABLE(id integer, public_id character varying, description text, media_type text, media_url text, vk_conversation text, vk_message_id bigint, discriminator text, tag_id integer, tag_name text, match_count integer)
 LANGUAGE plpgsql
AS $function$
BEGIN
    RETURN QUERY
    WITH combined_matches AS (
        -- Exact tag name matches
        SELECT 
            m.id AS media_id,
            t.id AS tag_id,
            t.""name"" AS tag_name,
            1 AS match_count
        FROM public.media_files m
        JOIN public.media_file_tag mft ON m.id = mft.media_files_id
        JOIN public.tags t ON t.id = mft.tags_id
        WHERE t.""name"" = __query

        UNION ALL
        
        -- 1/3 description matches
        SELECT 
            m.id AS media_id,
            NULL AS tag_id,
            NULL AS tag_name,
            1 AS match_count
        FROM public.media_files m
        WHERE m.description % __query
    )
    SELECT 
        m.id, 
        m.public_id, 
        m.description, 
        m.media_type, 
        m.media_url,
        v.vk_conversation, 
        v.vk_message_id,
        CASE WHEN v.id IS NOT NULL THEN 'VkVoiceMediaFile' END AS discriminator,
        cm.tag_id, 
        cm.tag_name, 
        cm.match_count
    FROM combined_matches cm
    JOIN media_files m ON m.id = cm.media_id
    LEFT JOIN vk_voice_media_file v ON m.id = v.id
    LEFT JOIN media_file_tag mft ON m.id = mft.media_files_id
    LEFT JOIN tags t ON t.id = mft.tags_id
    WHERE m.owner_id = __owner_id OR m.is_public = true
    ORDER BY m.id;
END;
$function$
;");
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("DROP FUNCTION find_media_by_tags_7");
		}
    }
}
