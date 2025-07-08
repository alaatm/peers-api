using System.Linq;
using Cake.Common;
using Cake.Core;

namespace Build.Extensions;

public static partial class BuildContextExtensions
{
    public static string GetRunnerIPAddress(this BuildContext context, int padLen)
    {
        if (!context.IsRunningInCI)
        {
            context.LogWarning("Skipping CI IP address retrieval. This operation is only supported in CI environments.", padLen);
            return string.Empty;
        }

        var exe = "curl";
        var args = "https://api.ipify.org";

        if (context.IsRunningOnWindows())
        {
            exe = "powershell";
            args = $"-Command \"(Invoke-WebRequest -Uri '{args}').Content\"";
        }

        RunProcess(context, exe, args, out var stdout);
        var ip = stdout.FirstOrDefault()?.Trim();

        if (string.IsNullOrEmpty(ip))
        {
            throw new CakeException("Failed to retrieve CI IP address.");
        }

        context.LogInformation($"CI IP address '{ip}'.", padLen);

        return ip;
    }
}
