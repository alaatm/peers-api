namespace Mashkoor.Modules.Media.Domain;

/// <summary>
/// Represents the category of media content.
/// </summary>
public enum MediaCategory
{
    /// <summary>
    /// A category that is not defined or recognized.
    /// </summary>
    Undefined = 0,
    /// <summary>
    /// An image file, such as JPEG, PNG, or GIF.
    /// </summary>
    Image,
    /// <summary>
    /// A video file, such as MP4, AVI, or MKV.
    /// </summary>
    Video,
    /// <summary>
    /// An audio file, such as MP3, WAV, or AAC.
    /// </summary>
    Audio,
    /// <summary>
    /// A document file, such as PDF, DOCX, or TXT.
    /// </summary>
    Document,
    /// <summary>
    /// A file that does not fit into the other categories, such as a ZIP archive or a custom file type.
    /// </summary>
    Other,
}
