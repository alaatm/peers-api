namespace Peers.Modules.Listings.Domain;

/// <summary>
/// Specifies the possible states of a listing throughout its lifecycle.
/// </summary>
public enum ListingState
{
    /// <summary>
    /// Indicates that the item is in a draft state and has not been finalized.
    /// </summary>
    Draft = 0,
    /// <summary>
    /// Specifies that the item is in the review state.
    /// </summary>
    Review = 1,
    /// <summary>
    /// Indicates that the item has been published and is visible to its intended audience.
    /// </summary>
    Published = 2,
    /// <summary>
    /// Indicates that the item is archived and no longer active.
    /// </summary>
    Archived = 3,
}
