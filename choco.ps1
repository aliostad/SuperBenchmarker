cd chocolatey

cpack SuperBenchmarkerCore.nuspec
cpack SuperBenchmarker.nuspec

gci *.nupkg | %{
    Write-Host Push $_
    cpush $_
    rm $_
}

cd ..