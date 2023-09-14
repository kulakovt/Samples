function Format-TeamCityParameter([string] $Value = $(throw "Value required"))
{
    # https://confluence.jetbrains.com/display/TCD10/Build+Script+Interaction+with+TeamCity#BuildScriptInteractionwithTeamCity-Escapedvalues
    $Value = $Value.Replace('|', '||')
    $Value = $Value.Replace("'", "|'")
    $Value = $Value.Replace('\n', '|n')
    $Value = $Value.Replace('\r', '|r')
    $Value = $Value.Replace('[', '|[')
    $Value = $Value.Replace(']', '|]')
    $Value
}

function Set-TeamCityBuildNumber([string] $Number = $(throw "Number required"))
{
    Write-Host "##teamcity[buildNumber '$Number']"
}

function Publish-TeamCityArtifact([string] $Pattern = $(throw "Pattern required"))
{
    Write-Host "##teamcity[publishArtifacts '$Pattern']"
}

function Open-TeamCityBlock([string] $Name = $(throw "Name required"), [string] $Description = '')
{
    $decodedName = Format-TeamCityParameter -Value $Name
    $decodedDescription = Format-TeamCityParameter -Value $Description
    Write-Host "##teamcity[blockOpened name='$decodedName' description='$decodedDescription']"
}

function Close-TeamCityBlock([string] $Name = $(throw "Name required"))
{
    $decodedName = Format-TeamCityParameter -Value $Name
    Write-Host "##teamcity[blockClosed name='$decodedName']"
}

function Step {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory, Position = 0)]
        [ValidateNotNullOrEmpty()]
        [string] $Name,

        [Parameter(Mandatory, Position = 1)]
        [ValidateNotNull()]
        [scriptblock] $Action,

        [Parameter()]
        [bool] $SkipIf = $false
    )

    if ($SkipIf)
    {
        Write-Information "Skip: $Name"
        return
    }

    Open-TeamCityBlock -Name $Name
    Invoke-Command -ScriptBlock $Action
    Close-TeamCityBlock $Name
}
