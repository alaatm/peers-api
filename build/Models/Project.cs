using Cake.Core.IO;

namespace Build.Models;

public sealed class Project
{
    public required string Name { get; init; } = default!;
    public required FilePath Path { get; init; } = default!;
}
