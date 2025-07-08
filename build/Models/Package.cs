using Cake.Core.IO;

namespace Build.Models;

public sealed class Package
{
    public required string ProjectName { get; init; } = default!;
    public required DirectoryPath Root { get; init; } = default!;
    public required FilePath File { get; init; } = default!;
}
