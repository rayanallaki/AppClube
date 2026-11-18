$ErrorActionPreference = 'Stop'
$base = 'http://localhost:5080'
function Verificar($condicao, $mensagem) { if (-not $condicao) { throw "FALHOU: $mensagem" }; Write-Host "OK: $mensagem" }
function StatusErro($bloco) { try { & $bloco | Out-Null; return 200 } catch { if ($_.Exception.Response) { return [int]$_.Exception.Response.StatusCode }; throw } }
$saude = Invoke-RestMethod "$base/api/saude"
Verificar ($saude.status -eq 'ok') 'API responde'
$status = StatusErro { Invoke-RestMethod "$base/api/espacos" }
Verificar ($status -eq 401) 'Acesso sem sessao recusado'
$login = Invoke-RestMethod "$base/api/auth/login" -Method Post -ContentType 'application/json' -Body '{"login":"titular","senha":"Teste@123"}'
$headers = @{ Authorization = "Bearer $($login.token)" }
Verificar ([bool]$login.token) 'Login do titular'
$espacos = Invoke-RestMethod "$base/api/espacos" -Headers $headers
Verificar ($espacos.Count -ge 1) 'Espaco de demonstracao'
$inicio = [DateTimeOffset]::UtcNow.AddDays(5)
$corpo = @{ associacaoId='22222222-2222-2222-2222-222222222222'; espacoId='33333333-3333-3333-3333-333333333333'; inicio=$inicio.ToString('o'); fim=$inicio.AddHours(2).ToString('o'); finalidade='Teste' } | ConvertTo-Json
$reserva = Invoke-RestMethod "$base/api/reservas" -Method Post -Headers $headers -ContentType 'application/json' -Body $corpo
Verificar ($reserva.status -eq 'Confirmada') 'Reserva confirmada'
$status = StatusErro { Invoke-RestMethod "$base/api/reservas" -Method Post -Headers $headers -ContentType 'application/json' -Body $corpo }
Verificar ($status -eq 400) 'Conflito de reserva recusado'
$status = StatusErro { Invoke-RestMethod "$base/api/adesoes" -Headers $headers }
Verificar ($status -eq 403) 'Titular sem permissao de analisar adesoes'
Write-Host 'Teste basico finalizado. Ele não cobre todos os requisitos.'
