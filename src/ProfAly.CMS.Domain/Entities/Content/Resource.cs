using ContentTypeEnum = ProfAly.CMS.Domain.Enums.ContentType;

namespace ProfAly.CMS.Domain.Entities.Content;

/// <summary>
/// A downloadable or linked resource (doc 08 §6): file-backed (<see cref="ContentItem.PdfFile"/>)
/// or link-backed (<see cref="ContentItem.ExternalUrl"/>).
/// </summary>
public class Resource : ContentItem
{
    /// <summary>Resource-type label (non-translatable).</summary>
    public string? ResourceType { get; set; }

    public override ContentTypeEnum ContentType => ContentTypeEnum.Resource;
}
