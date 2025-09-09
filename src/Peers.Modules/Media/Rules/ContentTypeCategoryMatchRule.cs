using Peers.Core.Domain.Rules;
using Peers.Modules.Media.Domain;

namespace Peers.Modules.Media.Rules;

public sealed class ContentTypeCategoryMatchRule : BusinessRule
{
    private readonly IStringLocalizer _l;
    private readonly string _contentType;
    private readonly MediaCategory _mediaCategory;

    public override string ErrorTitle => _l["Error adding media"];

    public ContentTypeCategoryMatchRule(string contentType, MediaCategory mediaCategory)
    {
        _l = StringLocalizerFactory.Create(typeof(res));
        _contentType = contentType;
        _mediaCategory = mediaCategory;
    }

    public override bool IsBroken()
    {
        if (MediaMaps.MimeTypeToCategory[_contentType] != _mediaCategory)
        {
            return Append(_l["Content type '{0}' does not match category '{1}'", _contentType, _mediaCategory]);
        }

        return Errors.Any();
    }
}
