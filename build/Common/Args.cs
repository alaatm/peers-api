using System;
using Cake.Common;
using Cake.Core;

namespace Build.Common;

public sealed class Args
{
    private static readonly string _defaultConfiguration = "Debug";

    public bool IsRelease => BuildConfiguration.Equals("release", StringComparison.OrdinalIgnoreCase);
    public bool IsDebug => BuildConfiguration.Equals("debug", StringComparison.OrdinalIgnoreCase);

    public string Target { get; set; }
    public string BuildConfiguration { get; set; }
    public string TargetProject { get; set; }

    public string AzureResourceGroup { get; set; }
    public string AzureSqlServer { get; set; }
    public string AzureApp { get; set; }
    public string ConnString { get; set; }

    public Args(ICakeContext context, bool isRunningInCI)
    {
        Target = context.Argument("target", "Default").Trim();
        BuildConfiguration = NormalizeBuildConfiguration(context.Argument("configuration", _defaultConfiguration).Trim());
        TargetProject = context.Argument("target-proj", "").Trim();

        AzureResourceGroup = context.Argument("az-resource-group", "").Trim();
        AzureSqlServer = context.Argument("az-sql-server", "").Trim();
        AzureApp = context.Argument("az-app", "").Trim();
        ConnString = context.Argument("conn-string", "").Trim();

        ValidateArgs(isRunningInCI);
    }

    private static string NormalizeBuildConfiguration(string configuration)
    {
        if (configuration.Equals("debug", StringComparison.OrdinalIgnoreCase) ||
            configuration.Equals("release", StringComparison.OrdinalIgnoreCase))
        {
            return configuration.Equals("release", StringComparison.OrdinalIgnoreCase) ? "Release" : "Debug";
        }

        throw new CakeException("Invalid build configuration. Supported values are 'Debug' and 'Release'.");
    }

    private void ValidateArgs(bool isRunningInCI)
    {
        if (string.Equals(Target, TaskNames.Deploy, StringComparison.OrdinalIgnoreCase))
        {
            if (isRunningInCI)
            {
                if (string.IsNullOrEmpty(AzureResourceGroup) ||
                    string.IsNullOrEmpty(AzureSqlServer) ||
                    string.IsNullOrEmpty(AzureApp) ||
                    string.IsNullOrEmpty(ConnString))
                {
                    throw new CakeException("Deployment in CI environment requires 'az-resource-group', 'az-sql-server', 'az-app' and 'conn-string' arguments.");
                }

                if (IsDebug)
                {
                    throw new CakeException("Deployment in CI environment requires 'Release' configuration.");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(ConnString))
                {
                    throw new CakeException("Deployment in local environment requires 'conn-string' argument.");
                }
            }
        }
    }
}
