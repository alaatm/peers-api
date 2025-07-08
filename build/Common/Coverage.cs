using Build.Extensions;
using Cake.Core;
using Cake.Core.IO;

namespace Build.Common;

public sealed class Coverage
{
    private readonly Solution _sln;

    public DirectoryPath Root { get; set; }
    public DirectoryPath HistoryRoot { get; set; }
    public FilePath LcovFile { get; set; }

    public Coverage(Solution sln)
    {
        _sln = sln;
        Root = sln.Root.Combine(".coverage");
        HistoryRoot = sln.Root.Combine(".coverage-history");
        LcovFile = sln.Root.CombineWithFilePath("lcov.info");
    }

    public void LogVerbose(ICakeContext context)
    {
        var padLen = _sln.PadLength;

        context.LogDebug("Coverage File System Details:");
        context.LogDebug($"  {"Root".PadRight(padLen)}: {Root}");
        context.LogDebug($"  {"History Root".PadRight(padLen)}: {HistoryRoot}");
        context.LogDebug($"  {"Lcov File".PadRight(padLen)}: {LcovFile}");

        context.LogDebug("");
    }
}
