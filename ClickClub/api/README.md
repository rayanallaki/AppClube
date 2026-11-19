# ClickClub API

ASP.NET Core 8 + Entity Framework Core 8 + Pomelo/MySQL. Web e Mobile só
consomem esta API. O banco central, autorizações, auditoria e decisões de
portaria permanecem no servidor. Operações financeiras são simuladas.

## Executar

Na raiz do projeto, use `Configurar-MySql.ps1`, conforme o guia geral. Depois:

```powershell
cd api
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --urls http://localhost:5080
```

O aplicativo MAUI para Windows usa `http://localhost:5080/` por padrão.
O endereço pode ser alterado na engrenagem da tela de login.

- Saúde: `/api/saude`
- Documentação interativa em Development: `/swagger`
- Login: POST `/api/auth/login` com `login` e `senha`.
- Demais operações: cabeçalho `Authorization: Bearer TOKEN`.
- Cadastro público: POST `/api/adesoes`; clubes em GET `/api/publico/clubes`.

Os usuários de demonstração usam `Teste@123`: `admin`, `presidente`,
`tesoureiro`, `funcionario`, `porteiro`, `titular` e `dependente`. São criados
somente uma vez, em um banco vazio com população de demonstração habilitada.

## Fluxos

- Pessoas e atribuições por unidade, categorias, vínculos, dependentes e
  solicitações de adesão com decisão administrativa.
- Espaços, horários, interdições, agenda, reservas, convidados e autorização
  do titular para locação paga de dependentes.
- Faturas por competência (`POST /api/faturas/gerar`), quitação simulada,
  discriminação (`GET /api/faturas/{id}/detalhes`), transferência de locação
  com acréscimo único de 10%, cancelamento e créditos simulados.
- Turmas, aulas, capacidade, lista de espera e promoção manual; eventos e
  visitantes; avisos e ouvidoria com respostas e estado.
- Carteirinha com texto assinado, PNG e SVG. O Mobile pode guardar e exibir
  a credencial até expirar; a portaria confirma a situação online na API.
- Painel, relatórios por unidade/período e trilha de auditoria.

A prévia completa de transferência é POST
`/api/reservas/{id}/preparar-transferencia`: cria uma mensalidade futura se
necessário e retorna o destino e os valores, sem transferir a taxa. A
confirmação é POST `/api/reservas/{id}/transferir` com `faturaId` da prévia.

## Banco e atualização

Em Development as migrações pendentes são aplicadas no arranque. A segunda
migração preserva o valor base de faturas já existentes, descontando os itens
transferidos/créditos do total histórico. Nunca utilize `EnsureCreated` em
conjunto com estas migrações nem exclua o banco para atualizar a estrutura.

O adaptador de persistência usa um Store por requisição, uma transação e uma
trava MySQL por banco, conservando os fluxos do protótipo. Isso atende à carga
de uma demonstração acadêmica; consultas seletivas e normalização das
coleções internas JSON continuam sendo melhorias de arquitetura, sem mudar
as operações expostas aos clientes.
