using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Domain.Enums;
using ProfAly.CMS.Infrastructure.Persistence;
using ProfAly.CMS.Infrastructure.Storage;
using ProfileEntity = ProfAly.CMS.Domain.Entities.Profile;

namespace ProfAly.CMS.Web.Areas.Admin.Pages.Profile;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IMediaUploadService _media;
    private readonly IStringLocalizer<SharedResource> _t;

    public IndexModel(AppDbContext db, IMediaUploadService media, IStringLocalizer<SharedResource> t)
    {
        _db = db;
        _media = media;
        _t = t;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? PhotoPath { get; private set; }
    public string? ContactPhotoPath { get; private set; }
    public string? BioImagePath { get; private set; }
    public Dictionary<string, string?> CvPaths { get; private set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public IReadOnlyList<string> Cultures => SupportedCultures.All;

    public class InputModel
    {
        public string? DateOfBirth { get; set; }

        [EmailAddress(ErrorMessage = "Field_Email_Invalid")]
        public string? Email { get; set; }

        public string? Phone { get; set; }

        public IFormFile? PhotoFile { get; set; }

        public IFormFile? ContactPhotoFile { get; set; }

        public IFormFile? BioImageFile { get; set; }

        public List<TranslationInput> Translations { get; set; } = new();
    }

    public class TranslationInput
    {
        public string Culture { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? ShortName { get; set; }
        public string? Title { get; set; }
        public string? Positioning { get; set; }
        public string? ShortBio { get; set; }
        public string? FullBio { get; set; }
        public string? Nationality { get; set; }
        public string? MaritalStatus { get; set; }
        public string? Location { get; set; }
        public string? Languages { get; set; }
        public IFormFile? CvFile { get; set; }
    }

    public async Task OnGetAsync()
    {
        var profile = await LoadAsync();
        Input.DateOfBirth = profile?.DateOfBirth?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        Input.Email = profile?.Email;
        Input.Phone = profile?.Phone;
        PhotoPath = MediaPath(profile?.Photo);
        ContactPhotoPath = MediaPath(profile?.ContactPhoto);
        BioImagePath = MediaPath(profile?.BioImage);

        Input.Translations = Cultures.Select(c =>
        {
            var tr = profile?.Translations.FirstOrDefault(x => x.Culture == c);
            CvPaths[c] = MediaPath(tr?.CvFile);
            return new TranslationInput
            {
                Culture = c,
                FullName = tr?.FullName,
                ShortName = tr?.ShortName,
                Title = tr?.Title,
                Positioning = tr?.Positioning,
                ShortBio = tr?.ShortBio,
                FullBio = tr?.FullBio,
                Nationality = tr?.Nationality,
                MaritalStatus = tr?.MaritalStatus,
                Location = tr?.Location,
                Languages = tr?.Languages,
            };
        }).ToList();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Default-culture (Arabic) name + title are required to have a meaningful profile.
        var def = Input.Translations.FirstOrDefault(x => x.Culture == SupportedCultures.Default);
        if (def is null || string.IsNullOrWhiteSpace(def.FullName))
        {
            ModelState.AddModelError("Input.Translations[0].FullName", _t["Field_Required"]);
        }
        if (def is null || string.IsNullOrWhiteSpace(def.Title))
        {
            ModelState.AddModelError("Input.Translations[0].Title", _t["Field_Required"]);
        }

        if (!ModelState.IsValid)
        {
            await ReloadMediaAsync();
            return Page();
        }

        var profile = await LoadAsync() ?? new ProfileEntity { CreatedUtc = DateTime.UtcNow };
        var isNew = profile.Id == 0;

        profile.Email = Input.Email;
        profile.Phone = Input.Phone;
        profile.DateOfBirth = DateTime.TryParse(Input.DateOfBirth, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var dob) ? dob : null;
        profile.ModifiedUtc = DateTime.UtcNow;

        if (Input.PhotoFile is not null)
        {
            var result = await _media.UploadAsync(Input.PhotoFile, MediaKind.Image);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("Input.PhotoFile", _t[result.ErrorKey!]);
                await ReloadMediaAsync();
                return Page();
            }
            profile.PhotoMediaId = result.File!.Id;
        }

        if (Input.ContactPhotoFile is not null)
        {
            var result = await _media.UploadAsync(Input.ContactPhotoFile, MediaKind.Image);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("Input.ContactPhotoFile", _t[result.ErrorKey!]);
                await ReloadMediaAsync();
                return Page();
            }
            profile.ContactPhotoMediaId = result.File!.Id;
        }

        if (Input.BioImageFile is not null)
        {
            var result = await _media.UploadAsync(Input.BioImageFile, MediaKind.Image);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("Input.BioImageFile", _t[result.ErrorKey!]);
                await ReloadMediaAsync();
                return Page();
            }
            profile.BioImageMediaId = result.File!.Id;
        }

        if (isNew)
        {
            _db.Profile.Add(profile);
        }

        foreach (var input in Input.Translations)
        {
            var tr = profile.Translations.FirstOrDefault(x => x.Culture == input.Culture);
            if (tr is null)
            {
                tr = new ProfileTranslation { Culture = input.Culture };
                profile.Translations.Add(tr);
            }

            tr.FullName = input.FullName ?? string.Empty;
            tr.ShortName = input.ShortName;
            tr.Title = input.Title ?? string.Empty;
            tr.Positioning = input.Positioning;
            tr.ShortBio = input.ShortBio;
            tr.FullBio = input.FullBio;
            tr.Nationality = input.Nationality;
            tr.MaritalStatus = input.MaritalStatus;
            tr.Location = input.Location;
            tr.Languages = input.Languages;

            if (input.CvFile is not null)
            {
                var cv = await _media.UploadAsync(input.CvFile, MediaKind.Pdf);
                if (!cv.Succeeded)
                {
                    ModelState.AddModelError($"Input.Translations[{Cultures.ToList().IndexOf(input.Culture)}].CvFile", _t[cv.ErrorKey!]);
                    await ReloadMediaAsync();
                    return Page();
                }
                tr.CvFileId = cv.File!.Id;
            }
        }

        await _db.SaveChangesAsync();
        StatusMessage = _t["Saved"];
        return RedirectToPage();
    }

    private async Task<ProfileEntity?> LoadAsync() =>
        await _db.Profile
            .Include(p => p.Photo)
            .Include(p => p.ContactPhoto)
            .Include(p => p.BioImage)
            .Include(p => p.Translations).ThenInclude(t => t.CvFile)
            .FirstOrDefaultAsync();

    private async Task ReloadMediaAsync()
    {
        var profile = await LoadAsync();
        PhotoPath = MediaPath(profile?.Photo);
        ContactPhotoPath = MediaPath(profile?.ContactPhoto);
        BioImagePath = MediaPath(profile?.BioImage);
        foreach (var c in Cultures)
        {
            CvPaths[c] = MediaPath(profile?.Translations.FirstOrDefault(x => x.Culture == c)?.CvFile);
        }
    }

    private static string? MediaPath(MediaFile? media) =>
        media is null ? null : "/uploads/" + media.RelativePath;
}
