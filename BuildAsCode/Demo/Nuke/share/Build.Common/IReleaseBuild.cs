using Build.Common.Integration;
using Nuke.Common;
using Serilog;

namespace Build.Common;

public interface IReleaseBuild : IBaseBuild
{
    Target CreateRelease => _ => _
        .TryDependsOn<IDockerBuild>(x => x.PushDockerArtifacts)
        .TryDependsOn<INuGetBuild>(x => x.PushNuGetArtifacts)
        .Executes(() =>
        {
            // Put here any logic you need to create release using your CI/CD system API

            var version = this.ResolveVersion();
            Log.Information("Release {ReleaseName} was successfully created for service {ServiceName}",version.Version, ServiceName);
        });
}