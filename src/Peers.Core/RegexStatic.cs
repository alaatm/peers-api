using System.Text.RegularExpressions;

namespace Peers.Core;

public static partial class RegexStatic
{
    [GeneratedRegex(@"^\d{4}$", RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
    public static partial Regex OtpRegex();
    [GeneratedRegex(@"^(?<func>startswith|contains|endswith|lt|le|eq|ge|gt|ne)\((?<arg>[^)]+)\)$", RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
    public static partial Regex FilterRegex();
    [GeneratedRegex("^[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b([-a-zA-Z0-9()@:%_\\+.~#?&//=]*)$", RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
    public static partial Regex SmtpUriRegex();
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
    public static partial Regex EmailRegex();
    [GeneratedRegex(@"^\+9665\d{8}$", RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
    public static partial Regex PhoneNumberRegex();
    [GeneratedRegex("^(?:[A-Za-z0-9+\\/]{4})*(?:[A-Za-z0-9+\\/]{4}|[A-Za-z0-9+\\/]{3}=|[A-Za-z0-9+\\/]{2}={2})$", RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
    public static partial Regex Base64Regex();
    [GeneratedRegex("^https?:\\/\\/(www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b([-a-zA-Z0-9()@:%_\\+.~#?&//=]*)$", RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
    public static partial Regex UriRegex();
    [GeneratedRegex("^(?<major>\\d+)\\.(?<minor>\\d+)\\.(?<build>\\d+) \\((?<revision>\\d+)\\)$", RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
    public static partial Regex ClientVersionRegex();
    [GeneratedRegex(@"^[a-z0-9_]+$", RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
    public static partial Regex IsSnakeCaseRegex();
    [GeneratedRegex(@"^[12]\d{9}$", RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
    public static partial Regex NationalIdRegex();
}
