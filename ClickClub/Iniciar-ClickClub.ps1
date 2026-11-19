param(
    [int]$PortaApi = 5080,
    [int]$PortaWeb = 5081,
    [switch]$RedeLocal = $true,
    [switch]$NaoAbrirNavegador
)

$ErrorActionPreference = 'Stop'
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'Instale o SDK .NET 8 e reabra o terminal.' }
if ($PortaApi -eq $PortaWeb) { throw 'Use portas diferentes para API e Web.' }
$pasta = $PSScriptRoot
$logs = Join-Path $pasta 'logs'
New-Item -ItemType Directory -Path $logs -Force | Out-Null
$api = Join-Path $pasta 'api/ClickClub.Api.csproj'
$web = Join-Path $pasta 'web/ClickClub.Web.csproj'
if (-not (Test-Path $api) -or -not (Test-Path $web)) { throw 'Extraia o projeto completo antes de iniciar.' }

Write-Host 'Compilando API e Web. A primeira execucao restaura os pacotes do NuGet.'
& dotnet build $api --nologo
if ($LASTEXITCODE -ne 0) { throw 'A compilacao da API falhou.' }
& dotnet build $web --nologo
if ($LASTEXITCODE -ne 0) { throw 'A compilacao Web falhou.' }

$ambienteAnterior = $env:ASPNETCORE_ENVIRONMENT
$apiAnterior = $env:Api__BaseUrl
$processos = @()
try {
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:Api__BaseUrl = "http://localhost:$PortaApi/"
    $hostApi = if ($RedeLocal) { '0.0.0.0' } else { 'localhost' }
    $dllApi = Join-Path $pasta 'api/bin/Debug/net8.0/ClickClub.Api.dll'
    $dllWeb = Join-Path $pasta 'web/bin/Debug/net8.0/ClickClub.Web.dll'
    $apiProcesso = Start-Process -FilePath 'dotnet' -ArgumentList "`"$dllApi`" --urls http://${hostApi}:$PortaApi" -WorkingDirectory (Join-Path $pasta 'api') -PassThru -RedirectStandardOutput (Join-Path $logs 'api.log') -RedirectStandardError (Join-Path $logs 'api-erros.log')
    $processos += $apiProcesso
    $pronta = $false
    for ($i = 0; $i -lt 60; $i++) {
        if ($apiProcesso.HasExited) { throw 'A API encerrou. Confira logs/api-erros.log e a configuracao MySQL.' }
        try {
            $saude = Invoke-RestMethod "http://localhost:$PortaApi/api/saude" -TimeoutSec 2
            if ($saude.status -eq 'ok') { $pronta = $true; break }
        } catch { Start-Sleep -Milliseconds 500 }
    }
    if (-not $pronta) { throw 'A API nao respondeu. Confira logs/api.log e o MySQL.' }
    $webProcesso = Start-Process -FilePath 'dotnet' -ArgumentList "`"$dllWeb`" --urls http://localhost:$PortaWeb" -WorkingDirectory (Join-Path $pasta 'web') -PassThru -RedirectStandardOutput (Join-Path $logs 'web.log') -RedirectStandardError (Join-Path $logs 'web-erros.log')
    $processos += $webProcesso
    $webPronta = $false
    for ($i = 0; $i -lt 30; $i++) {
        if ($webProcesso.HasExited) { throw 'A Web encerrou. Confira logs/web-erros.log.' }
        try { Invoke-WebRequest "http://localhost:$PortaWeb" -UseBasicParsing -TimeoutSec 2 | Out-Null; $webPronta = $true; break } catch { Start-Sleep -Milliseconds 500 }
    }
    if (-not $webPronta) { throw 'A Web nao respondeu. Confira logs/web.log.' }
    Write-Host "Web: http://localhost:$PortaWeb"
    Write-Host "API: http://localhost:$PortaApi/api/saude"
    Write-Host "Servidor do Mobile: http://localhost:$PortaApi/"
    if (-not $NaoAbrirNavegador) { Start-Process "http://localhost:$PortaWeb" }
    Read-Host 'Pressione Enter para encerrar API e Web' | Out-Null
} finally {
    foreach ($processo in $processos) {
        if (-not $processo.HasExited) { Stop-Process -Id $processo.Id -ErrorAction SilentlyContinue }
    }
    $env:ASPNETCORE_ENVIRONMENT = $ambienteAnterior
    $env:Api__BaseUrl = $apiAnterior
}
