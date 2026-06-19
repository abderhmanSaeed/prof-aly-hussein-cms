using ProfAly.CMS.Domain.Common;
using ContentTypeEnum = ProfAly.CMS.Domain.Enums.ContentType;

namespace ProfAly.CMS.Domain.Entities.Content;

/// <summary>A peer-reviewed journal publication (doc 08 §3). Journal/Authors are translatable.</summary>
public class Publication : ContentItem
{
    /// <summary>Digital Object Identifier (non-translatable).</summary>
    public string? Doi { get; set; }

    public override ContentTypeEnum ContentType => ContentTypeEnum.Publication;

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
