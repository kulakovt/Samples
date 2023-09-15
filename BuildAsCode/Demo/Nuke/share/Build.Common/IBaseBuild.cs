using Build.Common.Integration;
using Nuke.Common;
using Nuke.Common.IO;

namespace Build.Common;

public interface IBaseBuild : INukeBuild
{
    string ServiceName { get; }

    int Major { get; }

    int Minor { get; }

    [Parameter("Build counter"), Required]
    string BuildCounter => this.GetValue(() => BuildCounter);

    [Parameter("Vcs number"), Required]
    string VcsNumber => this.GetValue(() => VcsNumber);

    [Parameter("Branch name"), Required]
    string Branch => this.GetValue(() => Branch);

    [Parameter("Pull request number"), Required]
    string PullRequestNumber => this.GetValue(() => PullRequestNumber);

    [Parameter("Pull request source branch"), Required]
    string PullRequestSourceBranch => this.GetValue(() => PullRequestSourceBranch);

    AbsolutePath ArtifactsPath => RootDirectory / ".artifacts";
}
