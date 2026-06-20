using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfAly.CMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAboutImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AboutImageMediaId",
                table: "Profile",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Profile_AboutImageMediaId",
                table: "Profile",
                column: "AboutImageMediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Profile_MediaFile_AboutImageMediaId",
                table: "Profile",
                column: "AboutImageMediaId",
                principalTable: "MediaFile",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profile_MediaFile_AboutImageMediaId",
                table: "Profile");

            migrationBuilder.DropIndex(
                name: "IX_Profile_AboutImageMediaId",
                table: "Profile");

            migrationBuilder.DropColumn(
                name: "AboutImageMediaId",
                table: "Profile");
        }
    }
}
