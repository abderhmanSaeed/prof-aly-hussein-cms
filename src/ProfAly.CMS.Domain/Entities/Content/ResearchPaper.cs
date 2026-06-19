using ProfAly.CMS.Domain.Common;
using ProfAly.CMS.Domain.Enums;

namespace ProfAly.CMS.Domain.Entities.Content;

/// <summary>
/// Academic research output (doc 08 §4). Shares academic metadata with Publication;
/// distinguished only by its content type/section. (Section label: "Research".)
/// </summary>
public class ResearchPaper : ContentItem
{
    public ResearchPaper() : base(ContentType.ResearchPaper)
    {
    }

    public string? Doi { get; set; }

    public override IReadOnlyList<string> Validate()
    {
        var errors = new List<string>(base.Validate());
        if (!ContentRules.IsValidDoi(Doi))
        {
            errors.Add("DOI is not in a valid format.");
        }

        return errors;
    }
}
