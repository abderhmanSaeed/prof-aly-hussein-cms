using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProfAly.CMS.Domain.Entities;
using ProfAly.CMS.Domain.Enums;
using ProfAly.CMS.Infrastructure.Persistence;

namespace ProfAly.CMS.Web.Pages.Public;

[AllowAnonymous]
public class TeachingModel : PublicPageModel
{
    public TeachingModel(AppDbContext db) : base(db) { }

    public List<Course> Undergraduate { get; private set; } = new();
    public List<Course> Graduate { get; private set; } = new();

    public async Task OnGetAsync()
    {
        await LoadChromeAsync();
        var courses = await Db.Course.Include(c => c.Translations).OrderBy(c => c.SortOrder).ToListAsync();
        Undergraduate = courses.Where(c => c.Level == CourseLevel.Undergraduate).ToList();
        Graduate = courses.Where(c => c.Level == CourseLevel.Graduate).ToList();
    }
}
