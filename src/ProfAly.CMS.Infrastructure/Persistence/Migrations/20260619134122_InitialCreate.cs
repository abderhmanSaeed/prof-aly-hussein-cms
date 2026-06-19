using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfAly.CMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContactMessage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 254, nullable: false),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Message = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactMessage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Level = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    Period = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Course", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExperienceEntry",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartDateUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndDateUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperienceEntry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaFile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StoredFileName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    OriginalFileName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                    RelativePath = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    MediaKind = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    SizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: true),
                    Height = table.Column<int>(type: "INTEGER", nullable: true),
                    AltText = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Membership",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Kind = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Membership", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PageSection",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PageKey = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageSection", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PageSeo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PageKey = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    MetaTitle = table.Column<string>(type: "TEXT", maxLength: 70, nullable: true),
                    MetaDescription = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
                    MetaKeywords = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageSeo", x => x.Id);
                    table.CheckConstraint("CK_PageSeo_Culture", "Culture IN ('ar','en','fr')");
                });

            migrationBuilder.CreateTable(
                name: "PageView",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageView", x => x.Id);
                    table.CheckConstraint("CK_PageView_Culture", "Culture IN ('ar','en','fr')");
                });

            migrationBuilder.CreateTable(
                name: "Qualification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Year = table.Column<int>(type: "INTEGER", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Qualification", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Redirect",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FromPath = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    ToPath = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    StatusCode = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Redirect", x => x.Id);
                    table.CheckConstraint("CK_Redirect_StatusCode", "StatusCode IN (301, 302)");
                });

            migrationBuilder.CreateTable(
                name: "Skill",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skill", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Value = table.Column<int>(type: "INTEGER", nullable: false),
                    Suffix = table.Column<string>(type: "TEXT", maxLength: 5, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatItem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Activity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActivityGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Activity_ActivityGroup_ActivityGroupId",
                        column: x => x.ActivityGroupId,
                        principalTable: "ActivityGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivityGroupTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActivityGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityGroupTranslation", x => x.Id);
                    table.CheckConstraint("CK_ActivityGroupTranslation_Culture", "Culture IN ('ar','en','fr')");
                    table.ForeignKey(
                        name: "FK_ActivityGroupTranslation_ActivityGroup_ActivityGroupId",
                        column: x => x.ActivityGroupId,
                        principalTable: "ActivityGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CategoryTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryTranslation", x => x.Id);
                    table.CheckConstraint("CK_CategoryTranslation_Culture", "Culture IN ('ar','en','fr')");
                    table.ForeignKey(
                        name: "FK_CategoryTranslation_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    CourseName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Institution = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseTranslation", x => x.Id);
                    table.CheckConstraint("CK_CourseTranslation_Culture", "Culture IN ('ar','en','fr')");
                    table.ForeignKey(
                        name: "FK_CourseTranslation_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExperienceEntryTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExperienceEntryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Organization = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    PeriodLabel = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperienceEntryTranslation", x => x.Id);
                    table.CheckConstraint("CK_ExperienceEntryTranslation_Culture", "Culture IN ('ar','en','fr')");
                    table.ForeignKey(
                        name: "FK_ExperienceEntryTranslation_ExperienceEntry_ExperienceEntryId",
                        column: x => x.ExperienceEntryId,
                        principalTable: "ExperienceEntry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CoverImageId = table.Column<int>(type: "INTEGER", nullable: true),
                    PdfFileId = table.Column<int>(type: "INTEGER", nullable: true),
                    ExternalUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PublicationYear = table.Column<int>(type: "INTEGER", nullable: true),
                    EventDateUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsPublished = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    IsFeatured = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    ViewCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    DownloadCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    ResourceType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ProjectStatus = table.Column<string>(type: "TEXT", maxLength: 12, nullable: true),
                    Role = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Doi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ResearchPaper_Doi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ResourceType1 = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DegreeLevel = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    RelationshipType = table.Column<string>(type: "TEXT", maxLength: 12, nullable: true),
                    YouTubeVideoId = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentItem_MediaFile_CoverImageId",
                        column: x => x.CoverImageId,
                        principalTable: "MediaFile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ContentItem_MediaFile_PdfFileId",
                        column: x => x.PdfFileId,
                        principalTable: "MediaFile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Credibility",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LogoMediaId = table.Column<int>(type: "INTEGER", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credibility", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Credibility_MediaFile_LogoMediaId",
                        column: x => x.LogoMediaId,
                        principalTable: "MediaFile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Profile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PhotoMediaId = table.Column<int>(type: "INTEGER", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 254, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Profile_MediaFile_PhotoMediaId",
                        column: x => x.PhotoMediaId,
                        principalTable: "MediaFile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DefaultCulture = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    DefaultTheme = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    LogoMediaId = table.Column<int>(type: "INTEGER", nullable: true),
                    FacebookUrl = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    WhatsAppNumber = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    ContactEmail = table.Column<string>(type: "TEXT", maxLength: 254, nullable: false),
                    PrimaryColor = table.Column<string>(type: "TEXT", maxLength: 9, nullable: true),
                    SecondaryColor = table.Column<string>(type: "TEXT", maxLength: 9, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                    table.CheckConstraint("CK_SiteSettings_DefaultCulture", "DefaultCulture IN ('ar','en','fr')");
                    table.ForeignKey(
                        name: "FK_SiteSettings_MediaFile_LogoMediaId",
                        column: x => x.LogoMediaId,
                        principalTable: "MediaFile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MembershipTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MembershipId = table.Column<int>(type: "INTEGER", nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipTranslation", x => x.Id);
                    table.CheckConstraint("CK_MembershipTranslation_Culture", "Culture IN ('ar','en','fr')");
                    table.ForeignKey(
                        name: "FK_MembershipTranslation_Membership_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "Membership",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PageSectionTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PageSectionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Heading = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Body = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageSectionTranslation", x => x.Id);
                    table.CheckConstraint("CK_PageSectionTranslation_Culture", "Culture IN ('ar','en','fr')");
                    table.ForeignKey(
                        name: "FK_PageSectionTranslation_PageSection_PageSectionId",
                        column: x => x.PageSectionId,
                        principalTable: "PageSection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QualificationTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QualificationId = table.Column<int>(type: "INTEGER", nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Degree = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Institution = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Grade = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualificationTranslation", x => x.Id);
                    table.CheckConstraint("CK_QualificationTranslation_Culture", "Culture IN ('ar','en','fr')");
                    table.ForeignKey(
                        name: "FK_QualificationTranslation_Qualification_QualificationId",
                        column: x => x.QualificationId,
                        principalTable: "Qualification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SkillTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SkillId = table.Column<int>(type: "INTEGER", nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillTranslation", x => x.Id);
                    table.CheckConstraint("CK_SkillTranslation_Culture", "Culture IN ('ar','en','fr')");
                    table.ForeignKey(
                        name: "FK_SkillTranslation_Skill_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skill",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatItemTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StatItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatItemTranslation", x => x.Id);
                    table.CheckConstraint("CK_StatItemTranslation_Culture", "Culture IN ('ar','en','fr')");
                    table.ForeignKey(
                        name: "FK_StatItemTranslation_StatItem_StatItemId",
                        column: x => x.StatItemId,
                        principalTable: "StatItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivityTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActivityId = table.Column<int>(type: "INTEGER", nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityTranslation", x => x.Id);
                    table.CheckConstraint("CK_ActivityTranslation_Culture", "Culture IN ('ar','en','fr')");
                    table.ForeignKey(
                        name: "FK_ActivityTranslation_Activity_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentEvent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContentItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    EventType = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentEvent", x => x.Id);
                    table.CheckConstraint("CK_ContentEvent_Culture", "Culture IN ('ar','en','fr')");
                    table.ForeignKey(
                        name: "FK_ContentEvent_ContentItem_ContentItemId",
                        column: x => x.ContentItemId,
                        principalTable: "ContentItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentItemCategory",
                columns: table => new
                {
                    ContentItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentItemCategory", x => new { x.ContentItemId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_ContentItemCategory_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentItemCategory_ContentItem_ContentItemId",
                        column: x => x.ContentItemId,
                        principalTable: "ContentItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentItemTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContentItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 600, nullable: true),
                    Body = table.Column<string>(type: "TEXT", nullable: true),
                    Journal = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    Authors = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Publisher = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    AuthorshipRole = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    ResearcherName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    MetaTitle = table.Column<string>(type: "TEXT", maxLength: 70, nullable: true),
                    MetaDescription = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
                    MetaKeywords = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentItemTranslation", x => x.Id);
                    table.CheckConstraint("CK_ContentItemTranslation_Culture", "Culture IN ('ar','en','fr')");
                    table.ForeignKey(
                        name: "FK_ContentItemTranslation_ContentItem_ContentItemId",
                        column: x => x.ContentItemId,
                        principalTable: "ContentItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CredibilityTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CredibilityId = table.Column<int>(type: "INTEGER", nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CredibilityTranslation", x => x.Id);
                    table.CheckConstraint("CK_CredibilityTranslation_Culture", "Culture IN ('ar','en','fr')");
                    table.ForeignKey(
                        name: "FK_CredibilityTranslation_Credibility_CredibilityId",
                        column: x => x.CredibilityId,
                        principalTable: "Credibility",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    ShortName = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Positioning = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    ShortBio = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    FullBio = table.Column<string>(type: "TEXT", nullable: true),
                    Nationality = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    MaritalStatus = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Location = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Languages = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    CvFileId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileTranslation", x => x.Id);
                    table.CheckConstraint("CK_ProfileTranslation_Culture", "Culture IN ('ar','en','fr')");
                    table.ForeignKey(
                        name: "FK_ProfileTranslation_MediaFile_CvFileId",
                        column: x => x.CvFileId,
                        principalTable: "MediaFile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProfileTranslation_Profile_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettingsTranslation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SiteSettingsId = table.Column<int>(type: "INTEGER", nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    SiteTitle = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    FooterText = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Tagline = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettingsTranslation", x => x.Id);
                    table.CheckConstraint("CK_SiteSettingsTranslation_Culture", "Culture IN ('ar','en','fr')");
                    table.ForeignKey(
                        name: "FK_SiteSettingsTranslation_SiteSettings_SiteSettingsId",
                        column: x => x.SiteSettingsId,
                        principalTable: "SiteSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activity_ActivityGroupId_SortOrder",
                table: "Activity",
                columns: new[] { "ActivityGroupId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityGroup_SortOrder",
                table: "ActivityGroup",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityGroupTranslation_ActivityGroupId_Culture",
                table: "ActivityGroupTranslation",
                columns: new[] { "ActivityGroupId", "Culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTranslation_ActivityId_Culture",
                table: "ActivityTranslation",
                columns: new[] { "ActivityId", "Culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Category_SortOrder",
                table: "Category",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryTranslation_CategoryId_Culture",
                table: "CategoryTranslation",
                columns: new[] { "CategoryId", "Culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoryTranslation_Culture_Slug",
                table: "CategoryTranslation",
                columns: new[] { "Culture", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessage_IsRead_CreatedUtc",
                table: "ContactMessage",
                columns: new[] { "IsRead", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentEvent_ContentItemId_EventType_CreatedUtc",
                table: "ContentEvent",
                columns: new[] { "ContentItemId", "EventType", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentItem_ContentType_IsFeatured_IsPublished",
                table: "ContentItem",
                columns: new[] { "ContentType", "IsFeatured", "IsPublished" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentItem_ContentType_IsPublished_SortOrder",
                table: "ContentItem",
                columns: new[] { "ContentType", "IsPublished", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentItem_CoverImageId",
                table: "ContentItem",
                column: "CoverImageId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentItem_PdfFileId",
                table: "ContentItem",
                column: "PdfFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentItem_RelationshipType_DegreeLevel",
                table: "ContentItem",
                columns: new[] { "RelationshipType", "DegreeLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentItemCategory_CategoryId",
                table: "ContentItemCategory",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentItemTranslation_ContentItemId_Culture",
                table: "ContentItemTranslation",
                columns: new[] { "ContentItemId", "Culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentItemTranslation_Culture_Slug",
                table: "ContentItemTranslation",
                columns: new[] { "Culture", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Course_Level_SortOrder",
                table: "Course",
                columns: new[] { "Level", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseTranslation_CourseId_Culture",
                table: "CourseTranslation",
                columns: new[] { "CourseId", "Culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Credibility_LogoMediaId",
                table: "Credibility",
                column: "LogoMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_Credibility_SortOrder",
                table: "Credibility",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_CredibilityTranslation_CredibilityId_Culture",
                table: "CredibilityTranslation",
                columns: new[] { "CredibilityId", "Culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExperienceEntry_SortOrder",
                table: "ExperienceEntry",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ExperienceEntryTranslation_ExperienceEntryId_Culture",
                table: "ExperienceEntryTranslation",
                columns: new[] { "ExperienceEntryId", "Culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaFile_MediaKind_CreatedUtc",
                table: "MediaFile",
                columns: new[] { "MediaKind", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MediaFile_StoredFileName",
                table: "MediaFile",
                column: "StoredFileName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Membership_Kind_SortOrder",
                table: "Membership",
                columns: new[] { "Kind", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MembershipTranslation_MembershipId_Culture",
                table: "MembershipTranslation",
                columns: new[] { "MembershipId", "Culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PageSection_PageKey",
                table: "PageSection",
                column: "PageKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PageSectionTranslation_PageSectionId_Culture",
                table: "PageSectionTranslation",
                columns: new[] { "PageSectionId", "Culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PageSeo_PageKey_Culture",
                table: "PageSeo",
                columns: new[] { "PageKey", "Culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PageView_CreatedUtc",
                table: "PageView",
                column: "CreatedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Profile_PhotoMediaId",
                table: "Profile",
                column: "PhotoMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileTranslation_CvFileId",
                table: "ProfileTranslation",
                column: "CvFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileTranslation_ProfileId_Culture",
                table: "ProfileTranslation",
                columns: new[] { "ProfileId", "Culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Qualification_SortOrder",
                table: "Qualification",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_QualificationTranslation_QualificationId_Culture",
                table: "QualificationTranslation",
                columns: new[] { "QualificationId", "Culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Redirect_FromPath",
                table: "Redirect",
                column: "FromPath",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteSettings_LogoMediaId",
                table: "SiteSettings",
                column: "LogoMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteSettingsTranslation_SiteSettingsId_Culture",
                table: "SiteSettingsTranslation",
                columns: new[] { "SiteSettingsId", "Culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Skill_SortOrder",
                table: "Skill",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_SkillTranslation_SkillId_Culture",
                table: "SkillTranslation",
                columns: new[] { "SkillId", "Culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatItem_SortOrder",
                table: "StatItem",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_StatItemTranslation_StatItemId_Culture",
                table: "StatItemTranslation",
                columns: new[] { "StatItemId", "Culture" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityGroupTranslation");

            migrationBuilder.DropTable(
                name: "ActivityTranslation");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CategoryTranslation");

            migrationBuilder.DropTable(
                name: "ContactMessage");

            migrationBuilder.DropTable(
                name: "ContentEvent");

            migrationBuilder.DropTable(
                name: "ContentItemCategory");

            migrationBuilder.DropTable(
                name: "ContentItemTranslation");

            migrationBuilder.DropTable(
                name: "CourseTranslation");

            migrationBuilder.DropTable(
                name: "CredibilityTranslation");

            migrationBuilder.DropTable(
                name: "ExperienceEntryTranslation");

            migrationBuilder.DropTable(
                name: "MembershipTranslation");

            migrationBuilder.DropTable(
                name: "PageSectionTranslation");

            migrationBuilder.DropTable(
                name: "PageSeo");

            migrationBuilder.DropTable(
                name: "PageView");

            migrationBuilder.DropTable(
                name: "ProfileTranslation");

            migrationBuilder.DropTable(
                name: "QualificationTranslation");

            migrationBuilder.DropTable(
                name: "Redirect");

            migrationBuilder.DropTable(
                name: "SiteSettingsTranslation");

            migrationBuilder.DropTable(
                name: "SkillTranslation");

            migrationBuilder.DropTable(
                name: "StatItemTranslation");

            migrationBuilder.DropTable(
                name: "Activity");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "ContentItem");

            migrationBuilder.DropTable(
                name: "Course");

            migrationBuilder.DropTable(
                name: "Credibility");

            migrationBuilder.DropTable(
                name: "ExperienceEntry");

            migrationBuilder.DropTable(
                name: "Membership");

            migrationBuilder.DropTable(
                name: "PageSection");

            migrationBuilder.DropTable(
                name: "Profile");

            migrationBuilder.DropTable(
                name: "Qualification");

            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropTable(
                name: "Skill");

            migrationBuilder.DropTable(
                name: "StatItem");

            migrationBuilder.DropTable(
                name: "ActivityGroup");

            migrationBuilder.DropTable(
                name: "MediaFile");
        }
    }
}
