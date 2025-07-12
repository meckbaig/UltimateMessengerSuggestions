using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UltimateMessengerSuggestions.Migrations
{
    /// <inheritdoc />
    public partial class CreatePublicKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "public_id",
                table: "media_files",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);
			
			migrationBuilder.Sql(@"
				UPDATE media_files SET public_id = substring(md5(random()::text) for 8)
				WHERE public_id IS NULL;
			");

			migrationBuilder.AlterColumn<string>(
				name: "public_id",
				table: "media_files",
				type: "character varying(8)",
				maxLength: 8,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "character varying(8)",
				oldMaxLength: 8,
				oldNullable: true);

			migrationBuilder.CreateIndex(
                name: "ix_media_files_public_id",
                table: "media_files",
                column: "public_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_media_files_public_id",
                table: "media_files");

            migrationBuilder.DropColumn(
                name: "name",
                table: "users");

            migrationBuilder.DropColumn(
                name: "public_id",
                table: "media_files");
        }
    }
}
