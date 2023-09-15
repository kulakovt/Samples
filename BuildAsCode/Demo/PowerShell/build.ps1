#Requires -Version 5

<#
TeamCity arguments:
-Counter %build.counter%
-VcsNumber "%build.vcs.number%"
-Branch "%teamcity.build.branch%"
-PullRequestNumber "%teamcity.pullRequest.number%"
-PullRequestSourceBranch "%teamcity.pullRequest.source.branch%"
-DockerRepositoriesUrl "%DockerRepositoriesUrl%"
-DockerProjectName "%DockerProjectName%"
-NuGetUrl "%NuGetUrl%"
-NuGetFeedName "%NuGetFeedName%"
-NuGetKey "%NNuGetKey%"
-OctopusProjectsUrl "%OctopusProjectsUrl%"
-OctopusKey "%OctopusKey%"
-DotNetToolUrl "%DotNetToolUrl%"
#>

[CmdletBinding()]
param (
    [int]
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    $Counter,

    [string]
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    $VcsNumber,

    [string]
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    $Branch,

    [string]
    [Parameter()]
    [AllowNull()]
    [AllowEmptyString()]
    $PullRequestNumber = $null,

    [string]
    [Parameter()]
    [AllowNull()]
    [AllowEmptyString()]
    $PullRequestSourceBranch = $null,

    [Uri]
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    $DockerRepositoriesUrl,

    [string]
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    $DockerProjectName,

    [Uri]
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    $NuGetUrl,

    [string]
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    $NuGetFeedName,

    [string]
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    $NuGetKey,

    [Uri]
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    $OctopusProjectsUrl,

    [string]
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    $OctopusKey,

    [Uri]
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    $DotNetToolUrl,

    [Parameter(ValueFromRemainingArguments)]
    $UnboundArguments
)

Set-StrictMode -version Latest
$ErrorActionPreference = 'Stop'
$InformationPreference = 'Continue'

. $PSScriptRoot/build/TeamCity.ps1
. $PSScriptRoot/build/Builder.ps1

##################### Specification #####################

$ServiceName = "Buldac"
$ServiceMainProjectPath = "./src/Buldac/Buldac.csproj"
$ServiceMainAssemblyName = "Buldac.dll"

$DockerImageName = $ServiceName.ToLower()
$OctopusProjectName = $ServiceName

##################### Variables #####################

$ArtifactsPath = Join-Path $PSScriptRoot 'artifacts'

$DockerfilePath = Join-Path $PSScriptRoot 'build' | Join-Path -ChildPath 'Dockerfile'
# See Dockerfile, Publish and Final stages
$DockerContainerArtifactsPath = '/app/artifacts'
$DockerArtifactsPath = Join-Path $ArtifactsPath 'docker'

$NuGetArtifactsPath = Join-Path $ArtifactsPath 'nuget'
$NuGetPushUrl = $NuGetUrl | Join-Uri "nuget/${NuGetFeedName}/packages"

$OctopusArtifactsPath = Join-Path $ArtifactsPath 'octopus'
$OctopusUrl = [Uri]$OctopusProjectsUrl.GetLeftPart('Authority')

$PullRequestNumber, $PullRequestSourceBranch = Resolve-PullRequestLabels `
    -PullRequestNumber $PullRequestNumber `
    -PullRequestSourceBranch $PullRequestSourceBranch

##################### Version #####################

$major = 1
$minor = 2
$patch = $Counter

$preRelease = Format-PreReleaseSuffix `
    -Branch $Branch `
    -PullRequestNumber $PullRequestNumber `
    -PullRequestSourceBranch $PullRequestSourceBranch

$assemblyVersion = "${major}.${minor}"
$version = "${major}.${minor}.${patch}${preRelease}"
$fileVersion = "${major}.${minor}.${patch}"
$informationalVersion = "${version}+commit.${VcsNumber}"

$DockerImageNameTag = "$($DockerRepositoriesUrl.Authority)/${DockerProjectName}/${DockerImageName}:${version}"

Set-TeamCityBuildNumber -Number $version

Write-Information "Detailed Version: $informationalVersion"
Write-Information "Docker image name: $DockerImageNameTag"

$isPullRequest = [String]::IsNullOrWhiteSpace($PullRequestNumber) -eq $false

##################### Steps #####################

Step 'Build Dockerfile' {

    $ServiceMainProjectPath, $ServiceMainAssemblyName | Test-DockerfileContent -Path "$DockerfilePath"

    docker build `
        --tag "$DockerImageNameTag" `
        --pull `
        --progress plain `
        --file "$DockerfilePath" `
        --target final `
        --build-arg Version="$version" `
        --build-arg AssemblyVersion="$assemblyVersion" `
        --build-arg FileVersion="$fileVersion" `
        --build-arg InformationalVersion="$informationalVersion" `
        .

    Test-ExitCode 'docker build'

    Copy-ItemsFromDockerImage `
        -ImageNameTag "$DockerImageNameTag" `
        -Source "$DockerContainerArtifactsPath/." `
        -Destination "$ArtifactsPath"
}

Step 'Push Docker artifacts' -SkipIf $isPullRequest {

    docker push "$DockerImageNameTag"
    Test-ExitCode 'docker push'

    Out-DockerRedirectFile -Destination "$DockerArtifactsPath" -RepositoriesUrl "$DockerRepositoriesUrl" -ImageName "$DockerImageName" -Version "$version"
    Publish-TeamCityArtifact -Pattern "+:$(Join-Path $DockerArtifactsPath '*.html') => Docker"
}

Step 'Push NuGet artifacts' -SkipIf $isPullRequest {

    $packageFilter = Join-Path "$NuGetArtifactsPath" '*.nupkg'

    dotnet nuget push `
        "$packageFilter" `
        --source "$NuGetPushUrl" `
        --api-key "$NuGetKey" `
        --skip-duplicate `
        --force-english-output `

    Test-ExitCode 'nuget push'

    Get-ChildItem -Path $packageFilter |
    Out-NuGetRedirectFile -Url $NuGetUrl -FeedName $NuGetFeedName
    Publish-TeamCityArtifact -Pattern "+:$(Join-Path $NuGetArtifactsPath '*.html') => NuGet"
}

Step 'Create Octopus Release' -SkipIf $isPullRequest {

    New-OctopusRelease `
        -ServerUrl "$OctopusUrl" `
        -ApiKey "$OctopusKey" `
        -ProjectName "$OctopusProjectName" `
        -Version "$version" `
        -DotNetToolUrl "$DotNetToolUrl"

    Out-OctopusRedirectFile -Destination "$OctopusArtifactsPath" -ProjectsUrl "$OctopusProjectsUrl" -ProjectName "$OctopusProjectName" -Version "$version"
    Publish-TeamCityArtifact -Pattern "+:$(Join-Path $OctopusArtifactsPath '*.html') => Octopus"
}
