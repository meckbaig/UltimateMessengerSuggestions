using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UltimateMessengerSuggestions.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaOwnerwhip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_public",
                table: "media_files",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "owner_id",
                table: "media_files",
                type: "integer",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "ix_media_files_owner_id",
                table: "media_files",
                column: "owner_id");

            migrationBuilder.AddForeignKey(
                name: "fk_media_files_users_owner_id",
                table: "media_files",
                column: "owner_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.NoAction);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_media_files_users_owner_id",
                table: "media_files");

            migrationBuilder.DropIndex(
                name: "ix_media_files_owner_id",
                table: "media_files");

            migrationBuilder.DropColumn(
                name: "is_public",
                table: "media_files");

            migrationBuilder.DropColumn(
                name: "owner_id",
                table: "media_files");
        }
    }
}
