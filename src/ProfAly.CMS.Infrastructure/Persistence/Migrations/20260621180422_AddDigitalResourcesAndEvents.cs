using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfAly.CMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDigitalResourcesAndEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "ContentItemTranslation",
                type: "TEXT",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaseUrl",
                table: "ContentItem",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContentImage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContentItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    MediaFileId = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Caption = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentImage_ContentItem_ContentItemId",
                        column: x => x.ContentItemId,
                        principalTable: "ContentItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentImage_MediaFile_MediaFileId",
                        column: x => x.MediaFileId,
                        principalTable: "MediaFile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentImage_ContentItemId_SortOrder",
                table: "ContentImage",
                columns: new[] { "ContentItemId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentImage_MediaFileId",
                table: "ContentImage",
                column: "MediaFileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentImage");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "ContentItemTranslation");

            migrationBuilder.DropColumn(
                name: "PurchaseUrl",
                table: "ContentItem");
        }
    }
}
