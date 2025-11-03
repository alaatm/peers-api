namespace Peers.Modules.Media.Domain;

/// <summary>
/// Represents the type of media content.
/// </summary>
public enum MediaType
{
    /// <summary>
    /// A profile picture, typically used for user avatars or personal images.
    /// </summary>
    ProfilePicture,
    /// <summary>
    /// Represents an image associated with a product listing.
    /// </summary>
    ListingImage,
    /// <summary>
    /// Represents an image associated with a specific variant of a product listing.
    /// </summary>
    ListingVariantImage,
}
