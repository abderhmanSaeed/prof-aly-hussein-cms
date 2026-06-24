using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfAly.CMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContactIntro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactIntro",
                table: "ProfileTranslation",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactIntro",
                table: "ProfileTranslation");
        }
    }
}
