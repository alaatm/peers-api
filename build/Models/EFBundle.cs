using Cake.Core.IO;

namespace Build.Models;

public sealed class EFBundle
{
    public required string ProjectName { get; init; } = default!;
    public required FilePath File { get; init; } = default!;
}
