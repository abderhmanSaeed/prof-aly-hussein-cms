using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Infrastructure.Persistence;
using ProfAly.CMS.Web.Infrastructure;

namespace ProfAly.CMS.Web.Pages.Public;

[AllowAnonymous]
public class ContactModel : PublicPageModel
{
    private readonly IStringLocalizer<SharedResource> _t;

    public ContactModel(AppDbContext db, IStringLocalizer<SharedResource> t) : base(db) => _t = t;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ContactEmail { get; private set; }
    public string? ContactPhoneValue { get; private set; }
    public string? Location { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Field_Required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Field_Required")]
        [EmailAddress(ErrorMessage = "Field_Email_Invalid")]
        public string Email { get; set; } = string.Empty;

        public string? Subject { get; set; }

        [Required(ErrorMessage = "Field_Required")]
        public string Message { get; set; } = string.Empty;

        // Honeypot — must stay empty.
        public string? Website { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadInfoAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadInfoAsync();

        // Honeypot: silently accept (drop) bot submissions.
        if (!string.IsNullOrWhiteSpace(Input.Website))
        {
            StatusMessage = _t["Contact_Success"];
            return RedirectToPage(new { culture = Culture });
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        Db.ContactMessage.Add(new ContactMessage
        {
            Name = Input.Name.Trim(),
            Email = Input.Email.Trim(),
            Subject = string.IsNullOrWhiteSpace(Input.Subject) ? null : Input.Subject.Trim(),
            Message = Input.Message.Trim(),
            IsRead = false,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            CreatedUtc = DateTime.UtcNow,
        });
        await Db.SaveChangesAsync();

        StatusMessage = _t["Contact_Success"];
        return RedirectToPage(new { culture = Culture });
    }

    private async Task LoadInfoAsync()
    {
        await LoadChromeAsync();
        var settings = await Db.SiteSettings.FirstOrDefaultAsync();
        ContactEmail = settings?.ContactEmail;
        var profile = await Db.Profile.Include(p => p.Translations).FirstOrDefaultAsync();
        ContactPhoneValue = profile?.Phone;
        Location = Localized.Pick(profile?.Translations, Culture)?.Location;
    }
}
