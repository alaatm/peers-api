using System;
using System.Collections.Generic;
using Build.Extensions;
using Build.Models;
using Cake.Common;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.IO;

namespace Build.Common;

public sealed class Migrations
{
    private readonly Solution _sln;

    public FilePath ToolFile { get; set; }
    public DirectoryPath Root { get; set; }
    public List<EFBundle> BundleFiles { get; set; } = [];

    public Migrations(ICakeContext context, Solution sln)
    {
        _sln = sln;

        var bundleFileName = context.IsRunningOnWindows() ? "efbundle.exe" : "efbundle";
        var toolFileName = context.IsRunningOnWindows() ? "dotnet-ef.exe" : "dotnet-ef";

        Root = sln.Root.Combine(".migrations");
        foreach (var proj in sln.PublishableProjects)
        {
            var bundle = new EFBundle
            {
                ProjectName = proj.Main.Name,
                File = Root.Combine(new(proj.Main.Name)).CombineWithFilePath(bundleFileName),
            };
            BundleFiles.Add(bundle);
        }
        ToolFile = context.MakeAbsolute(context.Directory("tools")).CombineWithFilePath(toolFileName);
    }

    public EFBundle Get(string projectName)
    {
        var bundle = BundleFiles
            .Find(p => p.ProjectName.Equals(projectName, StringComparison.OrdinalIgnoreCase))
            ?? throw new CakeException($"Bundle for project '{projectName}' not found.");

        return bundle;
    }

    public void LogVerbose(ICakeContext context)
    {
        var padLen = _sln.PadLength;

        context.LogDebug("Migrations File System Details:");
        context.LogDebug($"  {"Root".PadRight(padLen)}: {Root}");
        foreach (var bundle in BundleFiles)
        {
            var name = $"Bundle File ({bundle.ProjectName})";
            context.LogDebug($"  {name.PadRight(padLen)}: {bundle.File}");
        }
        context.LogDebug($"  {"Tool File".PadRight(padLen)}: {ToolFile}");

        context.LogDebug("");
    }
}
