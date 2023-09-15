using Nuke.Common;

namespace Build.Common;

/// <summary>
/// Default build flow
/// </summary>
public interface IDefaultBuild :
    IDockerBuild,
    INuGetBuild,
    IReleaseBuild
{
    Target Default => _ => _
        .DependsOn<IReleaseBuild>(x => x.CreateRelease);

}

