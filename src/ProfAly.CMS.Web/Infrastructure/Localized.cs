using ProfAly.CMS.Domain.Common;

namespace ProfAly.CMS.Web.Infrastructure;

/// <summary>
/// Picks the translation row for the current culture with fallback to the default
/// culture (doc 10 §5). All public content is rendered from the database via this.
/// </summary>
public static class Localized
{
    public static T? Pick<T>(IEnumerable<T>? translations, string culture)
        where T : class, ITranslation
    {
        if (translations is null)
        {
            return null;
        }

        var list = translations as IList<T> ?? translations.ToList();
        return list.FirstOrDefault(t => t.Culture == culture)
               ?? list.FirstOrDefault(t => t.Culture == SupportedCultures.Default)
               ?? list.FirstOrDefault();
    }
}
