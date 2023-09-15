using Build.Common.Integration;
using Nuke.Common;
using Nuke.Common.IO;
using Serilog;

namespace Build.Common;

[ParameterPrefix(nameof(NuGet))]
public interface INuGetBuild: IBaseBuild
{
    [Parameter("NuGet url"), Required]
    Uri Url => this.GetValue(() => Url);

    [Parameter("NuGet feed name"), Required]
    string FeedName => this.GetValue(() => FeedName);

    //[Secret]
    [Parameter("NuGet API key"), Required]
    string ApiKey => this.GetValue(() => ApiKey);

    AbsolutePath NuGetArtifactsPath => ArtifactsPath / "nuget";
    
    Target PushNuGetArtifacts => _ => _
        .Requires(() => Url)
        .Requires(() => FeedName)
        .Requires(() => ApiKey)
        .TryDependsOn<IDockerBuild>(x => x.BuildDockerfile)
        .Executes(() =>
        {
            var nuGetPushUrl = Url.Combine($"nuget/{FeedName}/packages");

            // DotNetNuGetPush(settings =>
            //     settings
            //         .SetTargetPath(NuGetArtifactsPath / "*.nupkg")
            //         .SetSource(nuGetPushUrl.OriginalString)
            //         .SetApiKey(ApiKey)
            //         .EnableSkipDuplicate()
            //         .EnableForceEnglishOutput());

            var pushedArtifacts = NuGetArtifactsPath.GlobFiles("*.nupkg")
                .Select(x => x.Name)
                .ToList();
            
            Log.Information("Nuget artifacts {NuGetArtifacts} were successfully pushed to {NuGetUrl}", pushedArtifacts, nuGetPushUrl);
        });
}
