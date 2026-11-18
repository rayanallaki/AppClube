$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    foreach ($nome in @('ClickClub', 'ClickClub.Servidor')) {
        if (-not (Test-Path "$nome.sln")) {
            & dotnet new sln -n $nome
            if ($LASTEXITCODE -ne 0) { throw "Nao foi possivel criar $nome.sln" }
        }
    }
    $projetos = Get-ChildItem -Path api, web, mobile -Recurse -Filter '*.csproj' | Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' }
    foreach ($projeto in $projetos) {
        & dotnet sln ClickClub.sln add $projeto.FullName
        if ($LASTEXITCODE -ne 0) { throw 'Nao foi possivel adicionar projeto a solucao.' }
        if ($projeto.FullName -notmatch '[\\/]ClickClub\.Mobile[\\/]') {
            & dotnet sln ClickClub.Servidor.sln add $projeto.FullName
            if ($LASTEXITCODE -ne 0) { throw 'Nao foi possivel adicionar projeto a solucao servidor.' }
        }
    }
    Write-Host 'ClickClub.sln: todos os projetos (o app Android exige workloads e SDK Android).'
    Write-Host 'ClickClub.Servidor.sln: projetos que compilam sem o workload Android.'
} finally { Pop-Location }
