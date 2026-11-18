param(
    [string]$Servidor = 'localhost',
    [int]$Porta = 3306,
    [string]$Banco = 'clickclub_dev',
    [string]$Usuario = 'clickclub'
)

$ErrorActionPreference = 'Stop'
$projeto = Join-Path $PSScriptRoot 'api/ClickClub.Api.csproj'
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw 'Instale o SDK .NET 8 e abra um novo terminal antes de continuar.'
}
Write-Host "Configurando MySQL em ${Servidor}:$Porta, banco $Banco, usuario $Usuario."
$senhaSegura = Read-Host 'Senha do usuario MySQL (nao aparece ao digitar)' -AsSecureString
$senha = [System.Net.NetworkCredential]::new('', $senhaSegura).Password

# O builder faz o escape de ponto e vírgula/aspas na senha.
$connection = [System.Data.Common.DbConnectionStringBuilder]::new()
$connection['Server'] = $Servidor
$connection['Port'] = $Porta
$connection['Database'] = $Banco
$connection['User ID'] = $Usuario
$connection['Password'] = $senha
$connection['Connection Timeout'] = 10

# Reutiliza a chave existente: gerar outra invalidaria as carteirinhas emitidas.
$raw = (& dotnet user-secrets list --json --project $projeto) -join "`n"
if ($LASTEXITCODE -ne 0) { throw 'Nao foi possivel consultar a configuracao local do .NET.' }
$jsonExistente = ($raw -split "`n" | Where-Object { $_ -notmatch '^\s*//' }) -join "`n"
$existente = $jsonExistente | ConvertFrom-Json
$config = @{ 'ConnectionStrings:ClickClub' = $connection.ConnectionString }
if (-not $existente.'Credenciais:Chave') {
    $bytes = New-Object byte[] 32
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
    $config['Credenciais:Chave'] = [Convert]::ToBase64String($bytes)
}

# JSON pelo stdin evita colocar a senha nos argumentos do processo.
$encodingAnterior = $OutputEncoding
try {
    $OutputEncoding = [System.Text.UTF8Encoding]::new($false)
    $json = $config | ConvertTo-Json -Compress
    # ASCII com escapes JSON também funciona nas code pages do Windows PowerShell.
    $jsonAscii = [System.Text.RegularExpressions.Regex]::Replace($json, '[^\x00-\x7F]', {
        param($m)
        '\u{0:x4}' -f [int][char]$m.Value
    })
    $jsonAscii | & dotnet user-secrets set --project $projeto
    if ($LASTEXITCODE -ne 0) { throw 'Nao foi possivel salvar a configuracao local do .NET.' }
} finally {
    $OutputEncoding = $encodingAnterior
}
$senha = $null
$config = $null
$connection = $null
Write-Host 'Configuracao salva fora do projeto com .NET User Secrets.'
Write-Host 'Agora: cd api'
Write-Host '$env:ASPNETCORE_ENVIRONMENT = "Development"'
Write-Host 'dotnet run --urls http://localhost:5080'
