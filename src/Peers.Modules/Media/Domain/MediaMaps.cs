using System.Collections.Frozen;
using Peers.Modules.Customers.Domain;
using Peers.Modules.Listings.Domain;
using Peers.Modules.Ordering.Domain;

namespace Peers.Modules.Media.Domain;

public static class MediaMaps
{
    public static readonly FrozenDictionary<string, MediaCategory> MimeTypeToCategory =
        new Dictionary<string, MediaCategory>(StringComparer.OrdinalIgnoreCase)
        {
            // Images
            ["image/jpeg"] = MediaCategory.Image,
            ["image/png"] = MediaCategory.Image,

            // Videos
            ["video/mp4"] = MediaCategory.Video,
            ["video/quicktime"] = MediaCategory.Video,

            // Audio
            ["audio/m4a"] = MediaCategory.Audio,
            ["audio/mp4"] = MediaCategory.Audio,

            // Documents
            ["application/pdf"] = MediaCategory.Document,
            ["application/msword"] = MediaCategory.Document,
            ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"] = MediaCategory.Document,
            ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"] = MediaCategory.Document,
        }.ToFrozenDictionary();

    public static readonly FrozenDictionary<MediaType, MediaCategory> MediaTypeToCategory =
        new Dictionary<MediaType, MediaCategory>()
        {
            [MediaType.ProfilePicture] = MediaCategory.Image,
            [MediaType.ListingImage] = MediaCategory.Image,
            [MediaType.ListingVariantImage] = MediaCategory.Image,
            [MediaType.ShipmentProof] = MediaCategory.Image,
        }.ToFrozenDictionary();

    public static readonly FrozenDictionary<MediaType, string> MediaTypeToEntity =
        new Dictionary<MediaType, string>()
        {
            [MediaType.ProfilePicture] = nameof(Customer),
            [MediaType.ListingImage] = nameof(Listing),
            [MediaType.ListingVariantImage] = nameof(ListingVariant),
            [MediaType.ShipmentProof] = nameof(Shipment),
        }.ToFrozenDictionary();
}
