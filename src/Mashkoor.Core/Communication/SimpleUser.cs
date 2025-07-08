namespace Mashkoor.Core.Communication;

public sealed class SimpleUser
{
    public string PreferredLanguage { get; set; } = default!;
    public string? Email { get; set; }

#pragma warning disable CA1044 // Properties should not be write only - Intentionally write only
    public IEnumerable<string?> UserHandles
#pragma warning restore CA1044 // Properties should not be write only
    {
        set => Handles = value.Where(p => p is not null)!;
    }

    public IEnumerable<string> Handles { get; private set; } = default!;
}
