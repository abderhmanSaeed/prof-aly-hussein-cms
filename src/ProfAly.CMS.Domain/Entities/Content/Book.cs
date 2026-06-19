using ContentTypeEnum = ProfAly.CMS.Domain.Enums.ContentType;

namespace ProfAly.CMS.Domain.Entities.Content;

/// <summary>
/// A book/authored work (doc 08 §2). Publisher and authorship role are translatable
/// (on the translation row); featuring uses <see cref="ContentItem.IsFeatured"/>.
/// </summary>
public class Book : ContentItem
{
    public override ContentTypeEnum ContentType => ContentTypeEnum.Book;
}
