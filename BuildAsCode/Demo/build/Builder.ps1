function Format-PreReleaseSuffix(
    [string] $Branch = $(throw 'Branch Name required'),
    [string] $PullRequestNumber = $(throw 'Pull Request Number required'),
    [string] $PullRequestSourceBranch = $(throw 'Pull Request Source Branch required'))
{
    if ($Branch -eq 'master')
    {
        # pre-release suffix not required
        return ''
    }

    $name = $Branch
    $isPullRequest = [String]::IsNullOrWhiteSpace($PullRequestNumber) -eq $false
    if ($isPullRequest)
    {
        $name = "${PullRequestSourceBranch}-pr${PullRequestNumber}"
    }

    $suffix = $name -replace '[^0-9A-Za-z-]','-'

    if (($suffix.Length -eq 0) -or (-not [Char]::IsLetter($suffix[0])))
    {
        $suffix = 'pre' + $suffix
    }

    return "-$suffix"
}

function Resolve-PullRequestLabels(
    [string] $PullRequestNumber = $(throw 'Pull Request Number required'),
    [string] $PullRequestSourceBranch = $(throw 'Pull Request Source Branch required'))
{
    # TeamCity can't replace parameters if they don't exist
    if ($PullRequestNumber.Contains('%'))
    {
        # This is not a pull request
        return $null, $null
    }

    # Sometimes TeamCity can't provide the name of the source branch
    if ($PullRequestSourceBranch.Contains('%'))
    {
        $PullRequestSourceBranch = 'merge'
    }

    return $PullRequestNumber, $PullRequestSourceBranch
}

function Reset-Directory()
{
    process
    {
        $path = $_
        if (Test-Path -Path $path -PathType 'Container')
        {
            Remove-Item -Path $path -Recurse -Force
        }

        New-Item -ItemType Directory -Path $path -Force | Out-Null
    }
}

function Join-Uri([string] $RelativeUri = $(throw 'Relative Uri required'))
{
    process
    {
        $BaseUri = $_
        $left = $BaseUri.ToString().TrimEnd('/')
        $right = $RelativeUri.TrimStart('/')
        $separator = '/'

        if ($left.Contains('?') -or $right.StartsWith('?'))
        {
            $separator = ''
        }

        [Uri] "${left}${separator}${right}"
    }
}

function Copy-ItemsFromDockerImage(
    [string] $ImageNameTag = $(throw "Docker Image Name Tag required"),
    [string] $Source = $(throw "Source required"),
    [string] $Destination = $(throw "Destination required"))
{
    $Destination | Reset-Directory
    $containerId = docker container create "$ImageNameTag"
    Test-ExitCode 'docker create'
    $containerSource = "${containerId}:${Source}"

    docker container cp $containerSource $Destination
    Test-ExitCode 'docker cp'
    docker container rm --force $containerId
}

function New-OctopusRelease(
    [string] $ServerUrl = $(throw "Octopus server url required"),
    [string] $ApiKey = $(throw "Octopus API key required"),
    [string] $ProjectName = $(throw "Octopus project name required"),
    [string] $Version = $(throw "Version required"),
    [string] $DotNetToolUrl = $(throw "DotNet tool url required"))
{
    $OctopusCliExists = dotnet tool list -g | Select-String '^octopus.dotnet.fortis.cli\s+'
    if (-not $OctopusCliExists)
    {
        dotnet tool install --global --add-source "$DotNetToolUrl" "Octopus.DotNet.Fortis.Cli"
    }

    dotnet-fortis-octo create-release `
        --releaseNumber="$Version" `
        --server="$ServerUrl" `
        --apikey="$ApiKey" `
        --project="$ProjectName" `
        --latestbypublishdate `
        --ignoreexisting

    Test-ExitCode 'octo release'
}

function Out-HtmlRedirectFile(
    [Uri] $Url = $(throw 'Url required'),
    [string] $FilePath = $(throw 'File Path required'))
{
@"
<head>
  <meta http-equiv="refresh" content="1;URL=$Url" />
</head>
"@ |
      Out-File -Encoding UTF8 -FilePath $FilePath
}

function Out-DockerRedirectFile(
    [string] $Destination = $(throw 'Destination path required'),
    [Uri] $RepositoriesUrl = $(throw 'Docker repositories url required'),
    [string] $ImageName = $(throw 'Image name required'),
    [string] $Version = $(throw 'Version required'))
{
    $Destination | Reset-Directory
    $redirectUrl = $RepositoriesUrl | Join-Uri "${ImageName}/artifacts/${Version}"
    $htmlFilePath = Join-Path $Destination "${ImageName}-${Version}.html"
    Out-HtmlRedirectFile -Url $redirectUrl -FilePath $htmlFilePath
}

function Out-NuGetRedirectFile(
    [Uri] $Url = $(throw '$Url required'),
    [string] $FeedName = $(throw '$Feed Name required'))
{
    begin
    {
        # Based on https://github.com/semver/semver/blob/master/semver.md#is-there-a-suggested-regular-expression-regex-to-check-a-semver-string
        $semVerRegex = "^(?<PackageName>.*?).(?<PackageVersion>(?<Major>0|[1-9]\d*)\.(?<Minor>0|[1-9]\d*)\.(?<Patch>0|[1-9]\d*)(?:-(?<PreRelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<BuildMetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?)\.nupkg$"
    }
    process
    {
        $packageFile = $_
        if ($packageFile.Name -imatch $semVerRegex)
        {
            $packageName = $Matches.PackageName
            $packageVerison = $Matches.PackageVersion
            $redirectUrl = $Url | Join-Uri "feeds/${FeedName}/${packageName}/${packageVerison}"
            $htmlFilePath = "$($packageFile.FullName).html"
            Out-HtmlRedirectFile -Url $redirectUrl -FilePath $htmlFilePath
        }
    }
}

function Out-OctopusRedirectFile(
    [string] $Destination = $(throw 'Destination required'),
    [string] $ProjectsUrl = $(throw "Octopus projects url required"),
    [string] $ProjectName = $(throw "Octopus project name required"),
    [string] $Version = $(throw "Version required"))
{
    $Destination | Reset-Directory
    $redirectUrl = $ProjectsUrl | Join-Uri "${ProjectName}/deployments/releases/${Version}"
    $htmlFilePath = Join-Path $Destination "${ProjectName}-${Version}.html"
    Out-HtmlRedirectFile -Url $redirectUrl -FilePath $htmlFilePath
}

function Test-DockerfileContent([string] $Path = $(throw 'Path required'))
{
    begin
    {
        $hasError = $false
    }
    process
    {
        $pattern = $_
        $contains = Select-String -Path $Path -Pattern $pattern
        if ($contains)
        {
            return
        }

        $hasError = $true
        Write-Warning "«${pattern}» pattern was not found in $Path. Please check and replace all service specific variables."
    }
    end
    {
        if ($hasError)
        {
            throw 'Please, update Dockerfile'
        }
    }
}

function Test-ExitCode([string] $CommandName = 'Command')
{
    if ($LASTEXITCODE)
    {
        throw "«${CommandName}» as executed with an error (${LASTEXITCODE}). Please check the messages above."
    }
}
