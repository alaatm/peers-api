using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cake.Common;
using Cake.Core;
using Cake.Core.IO;

namespace Build.Extensions;

public static partial class BuildContextExtensions
{
    private static readonly Lock _lock = new();
    private static FilePath? _azPath;

    private static void Az(this BuildContext context, string args)
    {
        context.EnsureAzPath();
        Debug.Assert(_azPath is not null);

        context.RunProcess(_azPath, args, _azPath.GetDirectory());
    }

    private static void RunProcess(
        this BuildContext context,
        FilePath exe,
        string args)
        => context.RunProcess(exe: exe, args: args, workingDirectory: null, stdout: out _);

    private static void RunProcess(
        this BuildContext context,
        FilePath exe,
        string args,
        out IEnumerable<string> stdout)
        => context.RunProcess(exe: exe, args: args, workingDirectory: null, stdout: out stdout);

    private static void RunProcess(
        this BuildContext context,
        FilePath exe,
        string args,
        DirectoryPath workingDirectory)
        => context.RunProcess(exe: exe, args: args, workingDirectory: workingDirectory, stdout: out _);

    private static void RunProcess(
        this BuildContext context,
        FilePath exe,
        string args,
        DirectoryPath? workingDirectory,
        out IEnumerable<string> stdout)
    {
        var exitCode = context.StartProcess(exe,
            new ProcessSettings
            {
                Arguments = args,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            },
            out stdout,
            out var stderr);

        if (exitCode != 0)
        {
            var exeName = exe.GetFilename();
            var stdoutArray = stdout.ToArray();
            var stderrArray = stderr.ToArray();

            if (stderrArray.Length > 0)
            {
                foreach (var line in stderrArray)
                {
                    context.LogError($"[{exeName}]: {line}");
                }
            }
            else if (stdoutArray.Length > 0)
            {
                foreach (var line in stdoutArray)
                {
                    context.LogError($"[{exeName}]: {line}");
                }
            }

            throw new CakeException($"Failed to execute '{exe}' with args '{args}' in working dir '{workingDirectory ?? "N/A"}'{Environment.NewLine}(Exit code: {exitCode}).");
        }
    }

    private static void EnsureAzPath(this BuildContext context)
    {
        lock (_lock)
        {
            if (_azPath is null)
            {
                var azExe = context.IsRunningOnWindows()
                    ? "az.cmd"
                    : "az";

                if ((_azPath = context.Tools.Resolve(azExe)) is null)
                {
                    throw new CakeException($"az executable '{azExe}' not found.");
                }
            }
        }
    }
}
