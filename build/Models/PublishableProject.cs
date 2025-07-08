namespace Build.Models;

public sealed class PublishableProject
{
    public required Project Main { get; init; } = default!;
    public required Project Migrations { get; init; } = default!;
}
