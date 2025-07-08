using Cake.Core;
using Cake.Frosting;
using Build.Extensions;

namespace Build.Common;

public abstract class BuildContextBase : FrostingContext
{
    public bool IsRunningInCI => Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";

    public Args Args { get; set; }
    public Solution Solution { get; set; }
    public Coverage Coverage { get; set; }
    public PackOutput PackOutput { get; set; }
    public Migrations Migrations { get; set; }

    public BuildContextBase(ICakeContext context)
        : base(context)
    {
        Args = new Args(context, IsRunningInCI);
        Solution = new Solution(context);
        Coverage = new Coverage(Solution);
        PackOutput = new PackOutput(Solution);
        Migrations = new Migrations(context, Solution);

        context.LogInformation($"Build system starting '{(IsRunningInCI ? "CI" : "local")} run' in '{Args.BuildConfiguration}' configuration for target '{Args.Target}'.");
        if (!string.IsNullOrWhiteSpace(Args.TargetProject))
        {
            context.LogInformation($"Target project: '{Args.TargetProject}'.");
        }

        Solution.LogVerbose(context);
        Coverage.LogVerbose(context);
        PackOutput.LogVerbose(context);
        Migrations.LogVerbose(context);
    }
}
