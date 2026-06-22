using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Domain.Entities.Content;
using ProfAly.CMS.Domain.Enums;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Tests;

/// <summary>
/// Event video support (doc 90). Runs against a throwaway migrated SQLite file so the
/// AddEventVideo migration is exercised end-to-end. Verifies that (1) an Event persists
/// its own optional video id, (2) the video coexists with the image gallery, and (3) the
/// Event's video column is independent of the existing Video.YouTubeVideoId column.
/// </summary>
public class EventVideoTests : IDisposable
{
    private readonly string _dir;
    private readonly string _dbPath;

    public EventVideoTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "profaly-eventvideo-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
        _dbPath = Path.Combine(_dir, "app.db");
    }

    private AppDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options;
        return new AppDbContext(options);
    }

    private static MediaFile NewImage(string name) => new()
    {
        StoredFileName = name,
        OriginalFileName = name,
        RelativePath = "images/" + name,
        ContentType = "image/jpeg",
        MediaKind = MediaKind.Image,
        SizeBytes = 1024,
        CreatedUtc = DateTime.UtcNow,
    };

    [Fact]
    public async Task Event_PersistsVideoId_AndKeepsGallery()
    {
        await using (var ctx = NewContext())
        {
            await ctx.Database.MigrateAsync();

            var img1 = NewImage("a.jpg");
            var img2 = NewImage("b.jpg");
            ctx.AddRange(img1, img2);

            var ev = new Event
            {
                CreatedUtc = DateTime.UtcNow,
                IsPublished = true,
                VideoYouTubeId = "dQw4w9WgXcQ",
                Translations = { new ContentItemTranslation { Culture = "en", Title = "Sample Event", Slug = "sample-event" } },
                Images =
                {
                    new ContentImage { MediaFile = img1, SortOrder = 0 },
                    new ContentImage { MediaFile = img2, SortOrder = 1 },
                },
            };
            ctx.ContentItem.Add(ev);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = NewContext())
        {
            var ev = await ctx.ContentItem.OfType<Event>()
                .Include(e => e.Images)
                .SingleAsync();

            // Video id round-trips and the gallery coexists (doc 90 gallery rule: both shown).
            Assert.Equal("dQw4w9WgXcQ", ev.VideoYouTubeId);
            Assert.Equal(2, ev.Images.Count);
        }
    }

    [Fact]
    public async Task EventVideo_And_VideoYouTubeId_AreIndependentColumns()
    {
        await using (var ctx = NewContext())
        {
            await ctx.Database.MigrateAsync();

            ctx.ContentItem.Add(new Event
            {
                CreatedUtc = DateTime.UtcNow,
                VideoYouTubeId = "EVENTvideo1",
                Translations = { new ContentItemTranslation { Culture = "en", Title = "Ev", Slug = "ev" } },
            });
            ctx.ContentItem.Add(new Video
            {
                CreatedUtc = DateTime.UtcNow,
                YouTubeVideoId = "VIDEOvideo1",
                Translations = { new ContentItemTranslation { Culture = "en", Title = "Vid", Slug = "vid" } },
            });
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = NewContext())
        {
            var ev = await ctx.ContentItem.OfType<Event>().SingleAsync();
            var vid = await ctx.ContentItem.OfType<Video>().SingleAsync();

            // Each type reads back its own value — the columns do not collide.
            Assert.Equal("EVENTvideo1", ev.VideoYouTubeId);
            Assert.Equal("VIDEOvideo1", vid.YouTubeVideoId);
        }
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { Directory.Delete(_dir, recursive: true); } catch { /* temp cleanup best-effort */ }
        GC.SuppressFinalize(this);
    }
}
