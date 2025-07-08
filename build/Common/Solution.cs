using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Build.Extensions;
using Build.Models;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.IO;

namespace Build.Common;

public sealed class Solution
{
    public int PadLength { get; set; }

    public DirectoryPath Root { get; set; }
    public DirectoryPath SrcRoot { get; set; }
    public DirectoryPath TestRoot { get; set; }

    public FilePath Path { get; set; }
    public List<Project> Projects { get; set; } = [];
    public List<Project> TestProjects { get; set; } = [];
    public List<PublishableProject> PublishableProjects { get; set; } = [];

    public Solution(ICakeContext context)
    {
        Root = context.MakeAbsolute(context.Directory(".."));
        SrcRoot = Root.Combine("src");
        TestRoot = Root.Combine("test");

        var solutionFiles = context.GetFiles($"{Root}/*.{{sln,slnx}}");

        Path = solutionFiles.Count switch
        {
            0 => throw new CakeException("Solution file not found."),
            1 => solutionFiles.First(),
            _ => throw new CakeException("Multiple solution files found."),
        };

        var migrationsProjects = new Dictionary<string, FilePath>();
        var publishableProjects = new Dictionary<string, FilePath>();

        foreach (var proj in context.GetFiles($"{SrcRoot}/**/*.csproj"))
        {
            var projName = proj.GetFilenameWithoutExtension().ToString();
            Projects.Add(new Project { Name = projName, Path = proj });

            var migrationsDir = proj.GetDirectory().Combine("Migrations");
            if (context.DirectoryExists(migrationsDir))
            {
                migrationsProjects.Add(projName, proj);
            }

            if (IsProjectPublishable(proj))
            {
                publishableProjects.Add(projName, proj);
            }
        }

        foreach (var proj in context.GetFiles($"{TestRoot}/**/*.csproj"))
        {
            var projName = proj.GetFilenameWithoutExtension().ToString();
            TestProjects.Add(new Project { Name = projName, Path = proj });
        }

        if (Projects.Count == 0)
        {
            throw new CakeException("No source projects found.");
        }

        if (publishableProjects.Count == 0)
        {
            throw new CakeException("No publishable projects found.");
        }

        if (migrationsProjects.Count == 0)
        {
            throw new CakeException("No migrations projects found.");
        }

        foreach (var pubProj in publishableProjects)
        {
            var p = new Project { Name = pubProj.Key, Path = pubProj.Value };

            foreach (var migrationProject in migrationsProjects)
            {
                if (HasProjectReference(pubProj.Value, migrationProject.Key))
                {
                    var m = new Project { Name = migrationProject.Key, Path = migrationProject.Value };
                    PublishableProjects.Add(new PublishableProject
                    {
                        Main = p,
                        Migrations = m,
                    });
                }
            }
        }

        // Ensure each publishable project has exactly one migrations project that is referenced and that no two publishable projects reference the same migrations project.
        var migrationsProjectNames = new HashSet<string>(migrationsProjects.Keys);
        foreach (var publishableProject in PublishableProjects)
        {
            if (!migrationsProjectNames.Remove(publishableProject.Migrations.Name))
            {
                throw new CakeException($"Publishable project '{publishableProject.Main.Name}' references the same migrations project '{publishableProject.Migrations.Name}' as another publishable project.");
            }
        }

        if (PublishableProjects.Count != publishableProjects.Count)
        {
            throw new CakeException("Not all publishable projects have migrations projects.");
        }

        var tstPadLen = TestProjects.Count > 0
            ? TestProjects.Select(p => p.Name).Max(p => p.Length) + 1
            : 0;

        PadLength = Math.Max(
            Math.Max(
                Projects.Select(p => p.Name).Max(p => p.Length) + 1,
                tstPadLen),
            16) + 2 + 7;
    }

    public PublishableProject GetPublishableProject(string projectName)
    {
        var proj = PublishableProjects
            .Find(p => p.Main.Name.Equals(projectName, StringComparison.OrdinalIgnoreCase))
            ?? throw new CakeException($"Project '{projectName}' not found.");

        return proj;
    }

    public void LogVerbose(ICakeContext context)
    {
        context.LogDebug("Solution File System Details:");
        context.LogDebug($"  {"Root".PadRight(PadLength)}: {Root}");
        context.LogDebug($"  {"Src Root".PadRight(PadLength)}: {SrcRoot}");
        context.LogDebug($"  {"Test Root".PadRight(PadLength)}: {TestRoot}");

        context.LogDebug($"  {"Solution File".PadRight(PadLength)}: {Path}");
        context.LogDebug("");

        context.LogDebug($"  Projects ({Projects.Count})");
        foreach (var proj in Projects)
        {
            context.LogDebug($"  - {proj.Name.PadRight(PadLength - 2)}: {proj.Path}");
        }

        context.LogDebug("");

        context.LogDebug($"  Publishable Projects ({PublishableProjects.Count})");
        foreach (var proj in PublishableProjects)
        {
            context.LogDebug($"  - {proj.Main.Name.PadRight(PadLength - 2)}: {proj.Main.Path}");
            context.LogDebug($"  └─► (m) {proj.Migrations.Name.PadRight(PadLength - 8)}: {proj.Migrations.Path}");
        }

        context.LogDebug("");

        context.LogDebug($"  Test Projects ({TestProjects.Count})");
        foreach (var proj in TestProjects)
        {
            context.LogDebug($"  - {proj.Name.PadRight(PadLength - 2)}: {proj.Path}");
        }

        context.LogDebug("");
    }

    private static bool IsProjectPublishable(FilePath projectPath)
    {
        var doc = XDocument.Load(projectPath.FullPath);
        var ns = doc.Root?.Name.Namespace;

        var isPublishableValue = doc
            .Descendants($"{ns}IsPublishable")
            .FirstOrDefault()
            ?.Value;

        return string.Equals(isPublishableValue, "true", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasProjectReference(FilePath projectPath, string referencedProjectName)
    {
        var doc = XDocument.Load(projectPath.FullPath);
        var ns = doc.Root?.Name.Namespace;

        return doc
            .Descendants($"{ns}ProjectReference")
            .Any(pr =>
            {
                var include = pr.Attribute("Include")?.Value?.Replace('\\', '/');
                if (string.IsNullOrWhiteSpace(include))
                {
                    return false;
                }

                return System.IO.Path.GetFileNameWithoutExtension(include).Equals(
                    referencedProjectName,
                    StringComparison.OrdinalIgnoreCase);
            });
    }
}
