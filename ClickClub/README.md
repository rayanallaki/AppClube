# ClickClub — API, Web e MAUI para Windows

Projeto acadêmico de gestão de clubes com **API ASP.NET Core 8 + MySQL**,
**Web ASP.NET Core MVC** e **aplicativo .NET MAUI para Windows com SQLite local**.
Web e Mobile consomem a API; somente a API acessa o MySQL.

## Abrir e apresentar no Windows

1. No notebook, clone o repositório ou atualize a cópia local e abra a pasta
   `ClickClub`, onde está `ClickClub.sln`. Este projeto usa Windows x64,
   SDK .NET 10, workload `maui-windows` e Microsoft Edge WebView2 Runtime.
   A API e a Web precisam do ASP.NET Core Runtime 8 (x64).
2. Ligue o MySQL Server e prepare o banco local com o
   [guia do MySQL](INTEGRACAO_MYSQL.md). Para apresentar com os dados iniciais,
   use um schema vazio. Para levar os dados de um ensaio anterior, restaure
   um [backup do MySQL](banco/BACKUP_E_RESTAURACAO.md). O GitHub leva o código,
   e a configuração da conexão é feita em cada computador.
3. No PowerShell, na pasta `ClickClub`, configure a conexão local e inicie:

```powershell
powershell -ExecutionPolicy Bypass -File .\Configurar-MySql.ps1
powershell -ExecutionPolicy Bypass -File .\Iniciar-ClickClub.ps1
```

O primeiro script pede a senha no próprio terminal e salva conexão/chave em
.NET User Secrets, fora do projeto. O segundo compila API/Web, aguarda a
conexão com MySQL, abre a Web e encerra ambos ao pressionar Enter. Na primeira
execução em Development, as migrações criam as tabelas e populam um banco vazio
com dados fictícios. Reiniciar **preserva** os dados existentes.

- **Web:** http://localhost:5081
- **Saúde da API:** http://localhost:5080/api/saude
- **Mobile:** janela MAUI do Windows. Veja [como abrir](mobile/README.md).

Antes da apresentação, confirme que `/api/saude` retorna `status: ok`, abra
a Web e o Mobile e teste o login de demonstração. Faça a primeira compilação
com internet disponível para restaurar os pacotes do NuGet.

Para limitar a API ao computador, inicie o script com `-RedeLocal:$false`.

## Estrutura do ZIP

| Pasta/arquivo | Finalidade |
|---|---|
| `api` | API, autorização/regras, EF Core/Pomelo, MySQL e migrações. |
| `web` | Interface MVC de administração, serviços do associado e portaria. |
| `mobile` | Projeto único MAUI para Windows, com interface, cliente da API e cache SQLite. |
| `banco` | Criação do schema, SQL versionado e backup/restauração. |
| `testes` | Chamadas HTTP, verificações rápidas e suíte de integração. |
| `INTEGRACAO_MYSQL.md` | Configuração do servidor MySQL no Workbench. |
| `ROTEIRO_APRESENTACAO.md` | Sequência de demonstração com contas fictícias. |
| `TESTES_REALIZADOS.md` | Evidências e limites das verificações executadas. |

## Contas fictícias

As contas de demonstração usam a senha `Teste@123`: `admin`, `presidente`,
`tesoureiro`, `funcionario`, `porteiro`, `titular` e `dependente`. Cada perfil
recebe somente as operações permitidas pela API. O dependente precisa estar
vinculado à associação antes de usar serviços de sócio.

IDs originais: clube `11111111-1111-1111-1111-111111111111`, associação
`22222222-2222-2222-2222-222222222222` e salão
`33333333-3333-3333-3333-333333333333`. Não altere o banco da apresentação
para repetir testes. As contas são inseridas somente quando o banco está
vazio e a população de demonstração está habilitada.

## Executar manualmente

Em terminais PowerShell separados, na raiz:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project .\api\ClickClub.Api.csproj --urls http://localhost:5080
```

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:Api__BaseUrl = "http://localhost:5080/"
dotnet run --project .\web\ClickClub.Web.csproj --urls http://localhost:5081
```

No terceiro terminal, abra a janela do Mobile:

```powershell
cd .\mobile
dotnet run
```

Para gerar as soluções do Visual Studio, execute `Criar-Solucao.ps1`.
`ClickClub.sln` contém a API, a Web, o Mobile e seus testes.
`ClickClub.Servidor.sln` reúne somente a API e a Web.

## MySQL instalado ou Docker opcional

Se já usa MySQL Server/Workbench, siga `INTEGRACAO_MYSQL.md` e ignore o
Compose. Se preferir um servidor descartável de desenvolvimento via Docker,
copie `.env.example` para `.env`, escolha senhas próprias e execute
`docker compose up -d`. Não use a mesma porta de um MySQL já em execução.
A API continua usando a conexão configurada pelo script. Não envie o `.env`.

## Critérios e limites da entrega

Mensalidades, pagamentos, multas e portaria são **simulados**, conforme o
escopo acadêmico. O QR do associado pode ser exibido a partir do cache do
app, mas a portaria sempre depende da verificação online da API. Esta edição
do MAUI tem como destino o Windows. Push remoto e catracas físicas continuam
fora do escopo.

Consulte a matriz `MATRIZ_ENTREGA.md`, `PENDENCIAS.md` e o registro de testes
para distinguir os testes históricos da versão Android e as verificações
atuais do aplicativo para Windows.
