using System;
using System.Collections.Generic;
using Build.Extensions;
using Build.Models;
using Cake.Core;
using Cake.Core.IO;

namespace Build.Common;

public sealed class PackOutput
{
    private readonly Solution _sln;

    public DirectoryPath Root { get; set; }
    public List<Package> Packages { get; set; } = [];

    public PackOutput(Solution sln)
    {
        _sln = sln;
        Root = sln.Root.Combine(".publish");

        foreach (var proj in sln.PublishableProjects)
        {
            var package = new Package
            {
                ProjectName = proj.Main.Name,
                Root = Root.Combine(new(proj.Main.Name)),
                File = Root.CombineWithFilePath(new($"{proj.Main.Name}.zip")),
            };

            Packages.Add(package);
        }
    }

    public Package Get(string projectName)
    {
        var package = Packages
            .Find(p => p.ProjectName.Equals(projectName, StringComparison.OrdinalIgnoreCase))
            ?? throw new CakeException($"Package for project '{projectName}' not found.");

        return package;
    }

    public void LogVerbose(ICakeContext context)
    {
        var padLen = _sln.PadLength;

        context.LogDebug("Pack File System Details:");
        context.LogDebug($"  {"Root".PadRight(padLen)}: {Root}");
        foreach (var package in Packages)
        {
            var name = $"Package ({package.ProjectName})";
            context.LogDebug($"  {name.PadRight(padLen)}: {package.Root}");
            var arrow = $"└{new string('─', padLen - 3)}►";
            context.LogDebug($"  {arrow.PadRight(padLen - 1)} : {package.File}");
        }

        context.LogDebug("");
    }
}
