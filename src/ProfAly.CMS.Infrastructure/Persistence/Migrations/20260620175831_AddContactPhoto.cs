using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfAly.CMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContactPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContactPhotoMediaId",
                table: "Profile",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Profile_ContactPhotoMediaId",
                table: "Profile",
                column: "ContactPhotoMediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Profile_MediaFile_ContactPhotoMediaId",
                table: "Profile",
                column: "ContactPhotoMediaId",
                principalTable: "MediaFile",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profile_MediaFile_ContactPhotoMediaId",
                table: "Profile");

            migrationBuilder.DropIndex(
                name: "IX_Profile_ContactPhotoMediaId",
                table: "Profile");

            migrationBuilder.DropColumn(
                name: "ContactPhotoMediaId",
                table: "Profile");
        }
    }
}
