using System;
using System.IO;
using System.Linq;
using Build.Models;
using Cake.Common;
using Cake.Common.IO;
using Cake.Core;

namespace Build.Extensions;

public static partial class BuildContextExtensions
{
    private static readonly string _azFirewallRuleName = "BuildSystemRule";

    public static void CreateEfBundle(this BuildContext context, string projectName)
    {
        var proj = context.Solution.GetPublishableProject(projectName);
        CreateEfBundle(context, proj, 0);
    }

    public static void AzureDeploy(this BuildContext context, string projectName)
    {
        context.LogInformation("Deployment started.");

        var proj = context.Solution.GetPublishableProject(projectName);
        CreateEfBundle(context, proj, 2);
        ExecuteEfBundle(context, proj, 2);
        DeployToAzure(context, proj, 2);

        context.LogInformation("Deployment completed successfully.");
    }

    private static void CreateEfBundle(BuildContext context, PublishableProject project, int padLen)
    {
        context.LogInformation("Creating migrations bundle.", padLen);

        var rid = context.IsRunningOnWindows() ? "win-x64" : "linux-x64";

        var bundle = context.Migrations.Get(project.Main.Name);
        var bundleRoot = bundle.File.GetDirectory();

        var args = $"migrations bundle " +
            $"--self-contained " +
            $"--configuration {context.Args.BuildConfiguration} " +
            $"-s \"{project.Main.Path}\" " +
            $"-p \"{project.Migrations.Path}\" " +
            $"-r {rid} " +
            $"-o \"{bundle.File}\" " +
            $"--no-build";

        context.CreateDirectory(bundleRoot);

        context.RunProcess(context.Migrations.ToolFile, args);

        if (!context.FileExists(bundle.File))
        {
            throw new CakeException($"Failed to create migrations bundle. Expected output file '{bundle.File}' does not exist after running ef bundle command.");
        }

        context.LogInformation("Copying appsettings files to migrations directory.", padLen + 2);
        context.CopyFiles($"{project.Main.Path.GetDirectory()}/appsettings*.json", bundleRoot);

        context.LogInformation("Migration bundle created successfully.", padLen);
    }

    private static void ExecuteEfBundle(BuildContext context, PublishableProject project, int padLen)
    {
        context.LogInformation("Executing migrations bundle.", padLen);

        AddFirewallRule(context, padLen + 2);
        var bundle = context.Migrations.Get(project.Main.Name);

        try
        {
            context.RunProcess(
                bundle.File,
                $"--connection \"{context.Args.ConnString}\"",
                context.Migrations.Root,
                out var stdout);

            var appliedMigrations = stdout
                .Where(p => p.Contains("Applying migration", StringComparison.OrdinalIgnoreCase))
                .Select(p => p.Replace("Applying migration ", string.Empty).Trim())
                .ToArray();

            if (appliedMigrations.Length == 0)
            {
                context.LogInformation("No migrations were applied. The database is already up to date.", padLen + 2);
            }
            else
            {
                context.LogInformation("The following migration(s) were applied:", padLen + 2);
                foreach (var migration in appliedMigrations)
                {
                    context.LogInformation($"- {migration}", padLen + 2);
                }
            }
        }
        finally
        {
            DeleteFirewallRule(context, padLen + 2);
        }

        context.LogInformation("Migration bundle executed successfully.", padLen);
    }

    private static void DeployToAzure(BuildContext context, PublishableProject project, int padLen)
    {
        if (!context.IsRunningInCI)
        {
            context.LogWarning("Skipping Azure deployment. This operation is only supported in CI environments.", padLen);
            return;
        }

        context.LogInformation("Deploying to Azure.", padLen);
        var package = context.PackOutput.Get(project.Main.Name);

        var args = "webapp deploy " +
            $"--resource-group {context.Args.AzureResourceGroup} " +
            $"--name {context.Args.AzureApp} " +
            $"--src-path \"{package.File}\" " +
            $"--type zip";

        context.Az(args);

        context.LogInformation("App deployed successfully.", padLen);
    }

    private static void AddFirewallRule(BuildContext context, int padLen)
    {
        if (!context.IsRunningInCI)
        {
            context.LogWarning("Skipping firewall rule creation. This operation is only supported in CI environments.", padLen);
            return;
        }

        context.LogInformation("Adding firewall rule for the current runner.", padLen);

        var ciIPAddress = context.GetRunnerIPAddress(padLen);

        var args = "sql server firewall-rule create " +
            $"--resource-group {context.Args.AzureResourceGroup} " +
            $"--server {context.Args.AzureSqlServer} " +
            $"--name {_azFirewallRuleName} " +
            $"--start-ip-address {ciIPAddress} " +
            $"--end-ip-address {ciIPAddress}";

        context.Az(args);

        context.LogInformation("Firewall rule added successfully.", padLen);
    }

    private static void DeleteFirewallRule(BuildContext context, int padLen)
    {
        if (!context.IsRunningInCI)
        {
            context.LogWarning("Skipping firewall rule deletion. This operation is only supported in CI environments.", padLen);
            return;
        }

        context.LogInformation("Removing firewall rule for the current runner.", padLen);

        var args = "sql server firewall-rule delete " +
            $"--resource-group {context.Args.AzureResourceGroup} " +
            $"--server {context.Args.AzureSqlServer} " +
            $"--name {_azFirewallRuleName}";

        try
        {
            context.Az(args);
            context.LogInformation("Firewall rule removed successfully.", padLen);
        }
        catch
        {
            context.LogWarning("Failed to remove the firewall rule. Manual cleanup might be necessary.", padLen);
        }
    }
}
