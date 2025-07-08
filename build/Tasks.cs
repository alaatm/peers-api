using Build.Common;
using Build.Extensions;
using Cake.Frosting;

namespace Build;

[TaskName(TaskNames.Clean)]
public sealed class CleanTask : TaskBase
{
    public override void Run(BuildContext context) => context.CleanArtifacts();
}

[TaskName(TaskNames.Build)]
[IsDependentOn(typeof(CleanTask))]
public sealed class BuildTask : TaskBase
{
    public override void Run(BuildContext context) => context.BuildSolution();
}

[TaskName(TaskNames.Test)]
[IsDependentOn(typeof(BuildTask))]
public sealed class TestTask : TaskBase
{
    public override void Run(BuildContext context) => context.TestSolution();
}

[TaskName(TaskNames.Cover)]
[IsDependentOn(typeof(BuildTask))]
public sealed class CoverTask : TaskBase
{
    public override void Run(BuildContext context) => context.TestSolutionWithCoverage();
}

[TaskName(TaskNames.Migrations)]
[IsDependentOn(typeof(BuildTask))]
public sealed class MigrationsTask : TaskBase
{
    public override void Run(BuildContext context) => context.CreateEfBundle(context.Args.TargetProject);
}

[TaskName(TaskNames.Publish)]
[IsDependentOn(typeof(TestTask))]
public sealed class PublishTask : TaskBase
{
    public override void Run(BuildContext context) => context.PublishProject(context.Args.TargetProject);
}

[TaskName(TaskNames.Deploy)]
[IsDependentOn(typeof(PublishTask))]
public sealed class DeployTask : TaskBase
{
    public override void Run(BuildContext context) => context.AzureDeploy(context.Args.TargetProject);
}

[TaskName(TaskNames.Default)]
[IsDependentOn(typeof(CoverTask))]
public class DefaultTask : FrostingTask;
