using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfAly.CMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBioImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BioImageMediaId",
                table: "Profile",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Profile_BioImageMediaId",
                table: "Profile",
                column: "BioImageMediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Profile_MediaFile_BioImageMediaId",
                table: "Profile",
                column: "BioImageMediaId",
                principalTable: "MediaFile",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profile_MediaFile_BioImageMediaId",
                table: "Profile");

            migrationBuilder.DropIndex(
                name: "IX_Profile_BioImageMediaId",
                table: "Profile");

            migrationBuilder.DropColumn(
                name: "BioImageMediaId",
                table: "Profile");
        }
    }
}
