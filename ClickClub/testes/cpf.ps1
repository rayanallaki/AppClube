$ErrorActionPreference = 'Stop'
$base = 'http://localhost:5080'
$login = Invoke-RestMethod "$base/api/auth/login" -Method Post -ContentType 'application/json' -Body '{"login":"titular","senha":"Teste@123"}'
$headers = @{ Authorization = "Bearer $($login.token)" }
$usuario = Invoke-RestMethod "$base/api/auth/eu" -Headers $headers
$url = "$base/api/usuarios/$($usuario.id)/dados"

function AtualizarCpf([string]$cpf) {
  $body = @{ cpf=$cpf; interesses=@() } | ConvertTo-Json
  try {
    $resultado = Invoke-RestMethod $url -Method Put -Headers $headers -ContentType 'application/json' -Body $body
    return @{ sucesso=$true; resultado=$resultado }
  } catch {
    if ($_.Exception.Response.StatusCode.value__ -eq 400) { return @{ sucesso=$false } }
    throw
  }
}

if ((AtualizarCpf '111.111.111-11').sucesso) { throw 'CPF de digitos repetidos foi aceito.' }
if ((AtualizarCpf '529.982.247-26').sucesso) { throw 'CPF com digito verificador incorreto foi aceito.' }
$valido = AtualizarCpf '529.982.247-25'
if (-not $valido.sucesso -or $valido.resultado.cpf -ne '52998224725') { throw 'CPF valido nao foi aceito e normalizado.' }
Write-Host 'OK: CPF repetido rejeitado; verificador incorreto rejeitado; CPF valido normalizado.'
