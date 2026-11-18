# ClickClub — API, Web e Android

Projeto acadêmico de gestão de clubes com **API ASP.NET Core 8 + MySQL**,
**Web ASP.NET Core MVC** e **aplicativo Android .NET MAUI com SQLite local**.
Web e Mobile consomem a API; somente a API acessa o MySQL.

## Abrir e apresentar no Windows

1. Extraia o ZIP inteiro em uma pasta nova e abra `ClickClub` no VS Code ou
   no Visual Studio. Instale SDK .NET 8 e mantenha o MySQL Server ligado.
2. No MySQL Workbench, execute `banco/001_criar_banco.sql`. Crie o usuário
   `clickclub` com acesso somente a `clickclub_dev`, conforme o guia abaixo.
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
- **Android:** instruções em [GUIA_ANDROID.md](GUIA_ANDROID.md).
- **Instalador pronto:** `mobile/apk/ClickClub.apk` (Debug ARM64, Android 7 ou
  superior). A compilação e a assinatura foram verificadas; ensaie no telefone.
- **Prévia das telas mobile no computador:** projeto `mobile/Preview`, porta
  5082; instruções no guia Android. Usa a API e a interface compartilhada.

Por padrão, a API aceita conexões na rede local para o Android; a Web fica
em localhost. No telefone físico da mesma rede, informe no app
`http://IP-DO-COMPUTADOR:5080/`. No emulador Android padrão, use
`http://10.0.2.2:5080/`. Para limitar a API ao computador, inicie o script com
`-RedeLocal:$false`.

## Estrutura do ZIP

| Pasta/arquivo | Finalidade |
|---|---|
| `api` | API, autorização/regras, EF Core/Pomelo, MySQL e migrações. |
| `web` | Interface MVC de administração, serviços do associado e portaria. |
| `mobile` | Aplicativo MAUI Android, interface e serviços/cache SQLite. |
| `banco` | Criação do schema, SQL versionado e backup/restauração. |
| `testes` | Chamadas HTTP, verificações rápidas e suíte de integração. |
| `INTEGRACAO_MYSQL.md` | Configuração do servidor MySQL no Workbench. |
| `GUIA_ANDROID.md` | SDKs, compilação e acesso pela rede local. |
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

Para gerar as soluções do Visual Studio, execute `Criar-Solucao.ps1`.
`ClickClub.sln` contém todos os projetos, incluindo Android.
`ClickClub.Servidor.sln` reúne os projetos que não exigem o workload Android.
A compilação do app Android exige SDK/workload/JDK próprios, explicados no guia.

## MySQL instalado ou Docker opcional

Se já usa MySQL Server/Workbench, siga `INTEGRACAO_MYSQL.md` e ignore o
Compose. Se preferir um servidor descartável de desenvolvimento via Docker,
copie `.env.example` para `.env`, escolha senhas próprias e execute
`docker compose up -d`. Não use a mesma porta de um MySQL já em execução.
A API continua usando a conexão configurada pelo script. Não envie o `.env`.

## Critérios e limites da entrega

Mensalidades, pagamentos, multas e portaria são **simulados**, conforme o
escopo acadêmico. O QR do associado pode ser exibido a partir do cache do
app, mas a portaria sempre depende da verificação online da API. Push remoto,
iOS e catracas físicas permanecem fora do escopo aprovado.

Consulte a matriz `MATRIZ_ENTREGA.md`, `PENDENCIAS.md` e o registro de testes
para distinguir implementação, verificação em ambiente Linux e validação que
precisa ser repetida no Windows/telefone do apresentador. Não foram acessados
os arquivos do VS Code nem as credenciais MySQL do computador do usuário.
