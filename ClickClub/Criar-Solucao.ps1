$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    foreach ($nome in @('ClickClub', 'ClickClub.Servidor')) {
        if (-not (Test-Path "$nome.sln")) {
            & dotnet new sln -n $nome --format sln
            if ($LASTEXITCODE -ne 0) { throw "Nao foi possivel criar $nome.sln" }
        }
    }
    $servidor = @('api/ClickClub.Api.csproj', 'web/ClickClub.Web.csproj')
    $completa = $servidor + @('mobile/ClickClub.Mobile.csproj', 'testes/ClickClub.Mobile.Tests/ClickClub.Mobile.Tests.csproj')
    & dotnet sln ClickClub.sln add --in-root $completa
    if ($LASTEXITCODE -ne 0) { throw 'Nao foi possivel adicionar os projetos a solucao.' }
    & dotnet sln ClickClub.Servidor.sln add --in-root $servidor
    if ($LASTEXITCODE -ne 0) { throw 'Nao foi possivel adicionar os projetos a solucao servidor.' }
    Write-Host 'ClickClub.sln: API, Web, MAUI para Windows e testes do cliente.'
    Write-Host 'ClickClub.Servidor.sln: API e Web.'
} finally { Pop-Location }
