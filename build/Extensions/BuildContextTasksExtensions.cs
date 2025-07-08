using System.Linq;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Common.Tools.DotNet.Test;
using Cake.Common.Tools.ReportGenerator;
using Cake.Core;
using Cake.Core.IO;

namespace Build.Extensions;

public static partial class BuildContextExtensions
{
    public static void CleanArtifacts(this BuildContext context)
    {
        context.LogInformation("Cleaning artifacts.");
        context.CleanDirectories($"{context.Solution.SrcRoot}/**/bin/{context.Args.BuildConfiguration}");
        context.CleanDirectories($"{context.Solution.SrcRoot}/**/obj/{context.Args.BuildConfiguration}");
        context.CleanDirectories($"{context.Solution.TestRoot}/**/bin/{context.Args.BuildConfiguration}");
        context.CleanDirectories($"{context.Solution.TestRoot}/**/obj/{context.Args.BuildConfiguration}");
        context.CleanDirectory(context.Coverage.Root);
        context.CleanDirectory(context.PackOutput.Root);
        context.CleanDirectory(context.Migrations.Root);
        foreach (var package in context.PackOutput.Packages)
        {
            context.DeleteFileIfExists(package.File);
        }
    }

    public static void BuildSolution(this BuildContext context)
    {
        context.DotNetRestore(context.Solution.Root.FullPath);

        var settings = new DotNetBuildSettings
        {
            NoRestore = true,
            Configuration = context.Args.BuildConfiguration,
            MSBuildSettings = new DotNetMSBuildSettings(),
        };

        if (context.IsRunningInCI)
        {
            context.LogInformation("Using deterministic build settings.");
            settings.MSBuildSettings.WithProperty("ContinuousIntegrationBuild", "true");
            settings.MSBuildSettings.WithProperty("Deterministic", "true");
        }

        context.DotNetBuild(context.Solution.Root.FullPath, settings);
    }

    public static void TestSolution(this BuildContext context)
        => context.DotNetTest(context.Solution.Root.FullPath, new DotNetTestSettings
        {
            NoBuild = true,
            NoRestore = true,
            Configuration = context.Args.BuildConfiguration,
        });

    public static void TestSolutionWithCoverage(this BuildContext context)
    {
        const string CollectorConfigArg = "-- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration";

        if (context.Solution.TestProjects.Count == 0)
        {
            context.LogWarning("No test projects found. Skipping code coverage.");
            return;
        }

        var excludeTypes = string.Join(';', context.Solution.PublishableProjects.Select(p => $"[{p.Migrations.Name}]*Migrations.*"));

        foreach (var testProject in context.Solution.TestProjects)
        {
            context.DotNetTest(testProject.Path.FullPath, new DotNetTestSettings
            {
                NoBuild = true,
                NoRestore = true,
                Configuration = context.Args.BuildConfiguration,
                Collectors = ["XPlat Code Coverage"],
                ResultsDirectory = context.Coverage.Root,
                ArgumentCustomization = args => args
                    .Append($"{CollectorConfigArg}.Exclude={excludeTypes}")
                    .Append($"{CollectorConfigArg}.ExcludeByFile=**/obj/**/*.g.cs")
                    .Append($"{CollectorConfigArg}.ExcludeByAttribute=GeneratedCodeAttribute;CompilerGeneratedAttribute;ExcludeFromCodeCoverageAttribute")
            });
        }

        var coverageFiles = context.GetFiles($"{context.Coverage.Root}/**/coverage.cobertura.xml");

        if (coverageFiles.Count == 0)
        {
            throw new CakeException($"No coverage files found");
        }

        var reportPath = context.Coverage.Root.Combine("report");
        var mergedCoberturaFile = reportPath.CombineWithFilePath("Cobertura.xml");
        var mergedLcovFile = reportPath.CombineWithFilePath("lcov.info");

        context.ReportGenerator(
            reports: coverageFiles,
            targetDir: reportPath,
            new ReportGeneratorSettings
            {
                HistoryDirectory = context.Coverage.HistoryRoot,
                ReportTypes =
                [
                    ReportGeneratorReportType.Html,
                    ReportGeneratorReportType.Cobertura,
                    ReportGeneratorReportType.lcov,
                ],
            }
        );

        if (!context.FileExists(mergedCoberturaFile))
        {
            throw new CakeException($"Failed to merge code coverage output files. Expected output file '{mergedCoberturaFile}' does not exist after merging files");
        }

        if (!context.FileExists(mergedLcovFile))
        {
            throw new CakeException($"Failed to merge code coverage output files. Expected output file '{mergedLcovFile}' does not exist after merging files");
        }

        context.CopyFile(mergedLcovFile, context.Coverage.LcovFile);
    }

    public static void PublishProject(this BuildContext context, string projectName, DotNetMSBuildSettings? msBuildSettings = null)
    {
        var proj = context.Solution.GetPublishableProject(projectName);
        var package = context.PackOutput.Get(projectName);

        msBuildSettings ??= new DotNetMSBuildSettings();
        msBuildSettings.WithProperty("IsPublish", "true");

        var settings = new DotNetPublishSettings
        {
            // Rebuild is required to add IsPublish property to the project, can't add it to build as tests depending on
            // test-sys clock will fail.
            NoBuild = false,
            NoRestore = true,
            Configuration = context.Args.BuildConfiguration,
            MSBuildSettings = msBuildSettings,
            OutputDirectory = package.Root,
        };

        if (context.IsRunningInCI)
        {
            context.LogInformation("Using deterministic build settings.");
            settings.MSBuildSettings.WithProperty("ContinuousIntegrationBuild", "true");
            settings.MSBuildSettings.WithProperty("Deterministic", "true");
        }

        context.DotNetPublish(proj.Main.Path.FullPath, settings);

        context.LogInformation("Zipping package.");
        context.Zip(package.Root, package.File);
    }
}
