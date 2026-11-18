$ErrorActionPreference = 'Stop'
$base = 'http://localhost:5080'
function Teste($condicao, $mensagem) { if (-not $condicao) { throw "FALHOU: $mensagem" }; Write-Host "OK: $mensagem" }
function Post($path,$obj,$headers) { Invoke-RestMethod "$base$path" -Method Post -Headers $headers -ContentType 'application/json' -Body ($obj | ConvertTo-Json -Depth 10) }
function Login($nome) { $x=Post '/api/auth/login' @{login=$nome;senha='Teste@123'} @{}; return @{Authorization="Bearer $($x.token)"} }
$admin=Login 'admin'; $titular=Login 'titular'; $porteiro=Login 'porteiro'; $func=Login 'funcionario'; $dep=Login 'dependente'
$clube='11111111-1111-1111-1111-111111111111'; $associacao='22222222-2222-2222-2222-222222222222'; $espaco='33333333-3333-3333-3333-333333333333'
$depEu=Invoke-RestMethod "$base/api/auth/eu" -Headers $dep
$vinculo=Post "/api/associacoes/$associacao/dependentes" @{usuarioId=$depEu.id} $admin
Teste ($vinculo.dependentes -contains $depEu.id) 'Vinculo de dependente'
$cred=Post '/api/carteirinha' @{} $titular
Teste ([bool]$cred.credencial) 'Credencial emitida'
$acesso=Post '/api/portaria/validar' @{clubeId=$clube;credencial=$cred.credencial} $porteiro
Teste ($acesso.permitido -eq $true) 'Portaria valida credencial'
$negado=Post '/api/portaria/validar' @{clubeId=$clube;credencial='invalida'} $porteiro
Teste ($negado.permitido -eq $false) 'Credencial invalida recusada'
$inicio=[DateTimeOffset]::UtcNow.AddDays(10); $fim=$inicio.AddHours(2)
$r=Post '/api/reservas' @{associacaoId=$associacao;espacoId=$espaco;inicio=$inicio.ToString('o');fim=$fim.ToString('o');finalidade='Teste adicional'} $titular
$mes=[DateTime]::Today.AddMonths(1); $competencia=$mes.ToString('yyyy-MM');$vence=([DateTime]::new($mes.Year,$mes.Month,15)).ToString('yyyy-MM-dd')
$f=Post '/api/faturas' @{associacaoId=$associacao;competencia=$competencia;vencimento=$vence;valor=150} $admin
$previa=Invoke-RestMethod "$base/api/reservas/$($r.id)/transferencia-previa" -Headers $titular
Teste ($previa.total -eq 110) 'Previa de taxa com acrescimo de 10%'
$transf=Post "/api/reservas/$($r.id)/transferir" @{faturaId=$f.id} $titular
Teste ($transf.valor -eq 260) 'Transferencia para mensalidade'
$transf2=Post "/api/reservas/$($r.id)/transferir" @{faturaId=$f.id} $titular
Teste ($transf2.itens.Count -eq 2) 'Transferencia repetida nao duplica itens'
$cancelada=Post "/api/reservas/$($r.id)/cancelar" @{} $titular
$faturas=Invoke-RestMethod "$base/api/faturas" -Headers $titular
$atual=$faturas | Where-Object {$_.id -eq $f.id}
Teste ($atual.valor -eq 150) 'Cancelamento ajusta fatura aberta'
$t=Post '/api/turmas' @{clubeId=$clube;nome='Natacao de teste';inicio=$inicio.ToString('o');fim=$fim.ToString('o');capacidade=1} $func
$i=Post "/api/turmas/$($t.id)/inscricoes" @{} $titular
$j=Post "/api/turmas/$($t.id)/inscricoes" @{} $dep
Teste ($i.status -eq 'Confirmada' -and $j.status -eq 'Espera') 'Lotacao e lista de espera'
$null=Post "/api/inscricoes/$($i.id)/cancelar" @{} $titular
$promovido=Post "/api/turmas/$($t.id)/promover" @{} $func
Teste ($promovido.id -eq $j.id -and $promovido.status -eq 'Confirmada') 'Promocao manual da espera'
Write-Host 'Fluxos adicionais finalizados. Ainda existem requisitos pendentes; leia PENDENCIAS.md.'
