using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Docker;
using Serilog;
using static Nuke.Common.Tools.Docker.DockerTasks;

namespace Build.Common.Integration;

internal static class DockerBuildExtensions
{
    public static void BuildDocker(this IDockerBuild build)
    {
        Log.Information("Building docker image {DockerImageName}", build.DockerImageName);

        var version = build.ResolveVersion();
        
        DockerBuild(settings => settings
            .SetPath(build.RootDirectory)
            .SetFile(build.DockerfilePath)
            .SetTag(build.ResolveDockerImageNameTag())
            .EnablePull()
            .SetProgress(ProgressType.plain)
            .SetTarget("final")
            .SetBuildArg(
                $"Version={version.Version}",
                $"AssemblyVersion={version.AssemblyVersion}",
                $"FileVersion={version.FileVersion}",
                $"InformationalVersion={version.InformationalVersion}"));
    }

    public static string CreateDockerContainer(this IDockerBuild build)
    {
        Log.Information("Creating Docker container for {DockerImageName}", build.DockerImageName);
        
        var createResult = DockerContainerCreate(settings => settings
            .SetImage(build.ResolveDockerImageNameTag()));

        Assert.Count(createResult, 1);
        Assert.True(createResult.Single().Type == OutputType.Std);
        var containerId = createResult.Single().Text;
        containerId.NotNullOrWhiteSpace();

        return containerId;
    }

    public static void CopyArtifactsFromContainer(this IDockerBuild build, string containerId)
    {
        Log.Information("Copying items from Docker container {ContainerId}", containerId);

        var source = $"{IDockerBuild.DockerContainerArtifactsPath}/.";
        var destination = build.ArtifactsPath;

        var containerSource = $"{containerId}:{source}";
        Docker($"container cp {containerSource} {destination}");
    }

    public static void RemoveDockerContainer(this IDockerBuild build, string containerId)
    {
        Log.Information("Removing Docker container {ContainerId}", containerId);

        var removeResult = DockerContainerRm(settings => settings
            .SetContainers(containerId)
            .EnableForce());

        Assert.Count(removeResult, 1);
        Assert.True(String.Equals(removeResult.Single().Text, containerId, StringComparison.OrdinalIgnoreCase));
    }
}
