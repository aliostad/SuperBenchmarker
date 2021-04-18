del /F /Q .\artifacts\*.*
del /F /Q .\download\*.*
dotnet build SuperBenchmarker.sln
dotnet test .\test\SuperBenchmarker.Tests\SuperBenchmarker.Tests.csproj
dotnet build .\src\SuperBenchmarker\SuperBenchmarker.csproj -c Release -o .\artifacts\net452 -f net452
del /F /Q .\artifacts\net452\SuperBenchmarker.pdb
tools\ilmerge.exe /target:exe /ver:"4.6.0" /out:download\sb.exe /targetplatform:v4,%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319 artifacts\net452\SuperBenchmarker.exe artifacts\net452\CommandLine.dll artifacts\net452\Newtonsoft.Json.dll artifacts\net452\System.Net.Http.Formatting.dll artifacts\net452\RandomGen.dll
dotnet publish .\src\SuperBenchmarker\SuperBenchm6arker.csproj -c Release -o .\artifacts\netcoreapp2.1 -f netcoreapp2.1
dotnet publish .\src\SuperBenchmarker\SuperBenchmarker.csproj -c Release -o .\artifacts\netcoreapp3.1 -f netcoreapp3.1
mv artifacts\netcoreapp3.1\SuperBenchmarker.exe artifacts\netcoreapp3.1\sbcore.exe 