clear

$rootPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$server = Join-Path $rootPath "..\bin\Debug\PowerGraphNet.exe"
$logIn = Join-Path $rootPath "..\bin\Debug\Logs"
$logOut = Join-Path $rootPath "http-all.log"

if (Test-Path $logIn) { del $logIn -Recurse }

("Gateway", "Auth", "Static", "Text", "Image", "Ad") | % { start "$server" $_ }

clear
Invoke-RestMethod http://localhost:9000/index?slug

kill -ProcessName PowerGraphNet

cat "$logIn\*.log" > $logOut