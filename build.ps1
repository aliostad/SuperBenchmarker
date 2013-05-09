param(
    $buildFile   = (join-path (Split-Path -parent $MyInvocation.MyCommand.Definition) "SuperBenchmarker.msbuild"),
    $buildParams = "/p:Configuration=Release",
    $buildTarget = "/t:Default"
)

& "$(get-content env:windir)\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" $buildFile $buildParams $buildTarget
& "tools\ilmerge.exe" /target:exe /out:bin\sb.exe /targetplatform:'v4,C:\Windows\Microsoft.NET\Framework\v4.0.30319' artifacts\sb.exe artifacts\CommandLine.dll artifacts\Newtonsoft.Json.dll artifacts\System.Net.Http.Formatting.dll