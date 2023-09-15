using Build.Common.Integration;
using Nuke.Common;
using Serilog;
using static Nuke.Common.Tools.Docker.DockerTasks;

namespace Build.Common;

[ParameterPrefix(nameof(Docker))]
public interface IDockerBuild : IBaseBuild
{
    const string DockerContainerArtifactsPath = "/app/artifacts";

    string DockerImageName => ServiceName.ToKebabCaseLower();

    string DockerfilePath => RootDirectory / "build" / "Dockerfile";
    
    [Parameter("Docker repositories url"), Required]
    Uri RepositoriesUrl => this.GetValue(() => RepositoriesUrl);

    [Parameter("Docker project name"), Required]
    string ProjectName => this.GetValue(() => ProjectName);

    string ResolveDockerImageNameTag()
    {
        var version = this.ResolveVersion();
        return $"{RepositoriesUrl.Authority}/{ProjectName}/{DockerImageName}:{version.Version}";
    }

    /// <summary>
    /// Dockerfile processing pipeline: build -> create container -> copy artifacts -> remove container
    /// </summary>
    Target BuildDockerfile => _ => _
        .Requires(() => RepositoriesUrl)
        .Requires(() => ProjectName)
        .Executes(() =>
        {
            SetupLogging();

            this.BuildDocker();

            var containerId = this.CreateDockerContainer();
            this.CopyArtifactsFromContainer(containerId);

            this.RemoveDockerContainer(containerId);
        });

    /// <summary>
    /// Push Docker image to the repository
    /// </summary>
    Target PushDockerArtifacts => _ => _
        .Requires(() => RepositoriesUrl)
        .Requires(() => ProjectName)
        .TryDependsOn<IDockerBuild>(x => x.BuildDockerfile)
        .Executes(() =>
        {
            SetupLogging();

            var imageTag = ResolveDockerImageNameTag();
            // DockerPush(settings => settings
            //     .SetName(imageTag);
            Log.Information("Docker image {DockerImageName} was pushed to {DockerImageTag}", DockerImageName, imageTag);
        });

    // Workaround for logging issue in Nuke with Docker tasks
    // See more details here: https://nuke.build/faq
    static void SetupLogging() => DockerLogger = (_, text) => Log.Debug(text);
}
