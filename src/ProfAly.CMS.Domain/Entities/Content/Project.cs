using ProfAly.CMS.Domain.Enums;
using ContentTypeEnum = ProfAly.CMS.Domain.Enums.ContentType;

namespace ProfAly.CMS.Domain.Entities.Content;

/// <summary>A project (doc 08 §9). Outbound link via <see cref="ContentItem.ExternalUrl"/>.</summary>
public class Project : ContentItem
{
    public ProjectStatus ProjectStatus { get; set; }

    /// <summary>The professor's role on the project (non-translatable label).</summary>
    public string? Role { get; set; }

    public override ContentTypeEnum ContentType => ContentTypeEnum.Project;
}
