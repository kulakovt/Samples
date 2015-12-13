#requires -version 5

clear

$rootPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$logPath = Join-Path $rootPath "http-all.log"
$dotPath = Join-Path $rootPath "GraphViz\bin\dot.exe"
$gvPath = [IO.Path]::ChangeExtension($logPath, "gv")
$imgPath = [IO.Path]::ChangeExtension($logPath, "png")


##############
### Checks ###

if (-not (Test-Path $dotPath))
{
    Write-Error "Please, install GraphViz from http://www.graphviz.org"
    return
}

if (-not (Test-Path $logPath))
{
    Write-Error "Please, run Receive-Logs.ps1 first"
    return
}

################
### Read log ###

$json = @("[")
$json += cat $logPath
$json += "{}]" # suppress last comma

$log = $json | ConvertFrom-Json
$log = $log[0..($log.Length-2)] # remove last empty record

$requestId = $log |
    select -ExpandProperty Properties |
    where { $_.RequestQuery -eq 'slug' } |
    select -ExpandProperty AppRequestId -First 1

$log = $log | where { $_.Properties.AppRequestId -eq $requestId }

$log | foreach { $_.RenderedMessage }


##########################
### Get slow responses ###

$durations = @{}
$log |
    select -ExpandProperty Properties |
    where { $_.EventType -eq 'HttpResponse' } |
    foreach { $durations.Add($_.Host, ([TimeSpan]$_.RequestDuration).TotalMilliseconds) }

$slowHosts = $durations.GetEnumerator() |
        where { $_.Value -gt 1000 } |
        select -ExpandProperty Name


#########################
### Get request flows ###
$flows = $log |
    select -ExpandProperty Properties |
    where { $_.EventType -eq 'HttpRequest' } |
    select @{ Name="From"; Expression={$_.Referer} }, @{ N="To"; E={$_.Host} }, @{ N="Duration"; E={$durations[$_.Host]} }


###################
### Build graph ###
$graph =
    'digraph Requests {',
    '  rankdir="LR";',
    '  node [shape="box", style="filled", fillcolor="green", fontcolor="black"];',
    ''

$graph += $slowHosts |
    foreach { '  "{0}" [fillcolor="red"];' -f $_ }
$graph += ''

$graph += $flows |
    where { $_.From -ne $null } | # skip root
    foreach { '  "{0}" -> "{1}" [label="{2}ms"];' -f $_.From,$_.To,[int]$_.Duration }
$graph += '}'

$graph | Set-Content $gvPath

&$dotPath -Gdpi=256 -Tpng $gvPath "-o$imgPath"
&$imgPath
