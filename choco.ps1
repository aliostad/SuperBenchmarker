cd chocolatey

cpack

#nuget.exe setApiKey YOURS-VERY-OWN-API-KEY -Source http://chocolatey.org/
gci *.nupkg | %{
    Write-Host Push $_
    cpush $_
    rm $_
}

cd ..