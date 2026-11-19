# Conectar o ClickClub ao seu MySQL pelo VS Code

O MySQL Server guarda os dados. O Workbench é a ferramenta para administrar esse
servidor. O fluxo desta versão é: API C# → Entity Framework Core/Pomelo → MySQL.
A Web e o aplicativo Mobile acessam a API; o SQLite do app é um cache local, não uma réplica completa do MySQL.

## 1. Preparar um banco novo no Workbench

Abra a conexão local que você já usa. Anote o hostname e a porta exibidos na
conexão (normalmente `localhost` e `3306`). Em uma aba SQL, execute o
`CREATE DATABASE IF NOT EXISTS` de `banco/001_criar_banco.sql` para criar
`clickclub_dev`. Para um notebook com MySQL recém-instalado, crie o usuário
e conceda as permissões pelo Workbench, como descrito abaixo.

Em **Administration → Users and Privileges**, crie o usuário local `clickclub`,
defina sua própria senha e conceda permissões sobre **somente** `clickclub_dev`
em **Schema Privileges**. Para esta fase de desenvolvimento, as migrações
precisam criar/alterar tabelas, chaves e índices, além de ler/gravar registros.
O host do usuário deve permitir a conexão local que você utiliza.

Use um schema separado, vazio. A migração não importa tabelas de versões antigas
nem dados que estavam na memória de outra execução da API. Se você já tem um
schema ClickClub com tabelas/dados, preserve-o e utilize `clickclub_dev` para
testar esta versão.

## 2. Abrir esta versão no VS Code

Abra a pasta `ClickClub` no VS Code. Para executar API e Web, instale o
ASP.NET Core Runtime 8. Para compilar todos os projetos e abrir o Mobile,
use também o SDK .NET 10 conforme o [guia do Mobile](mobile/README.md).

No terminal **PowerShell**, dentro de `ClickClub`, execute:

```powershell
powershell -ExecutionPolicy Bypass -File .\Configurar-MySql.ps1
```

Digite a senha do usuário MySQL `clickclub` quando o script pedir. A senha não
é exibida. O script salva a conexão e uma chave de assinatura no **.NET User
Secrets**, fora da pasta do projeto. Não envie a senha no chat, não publique o
arquivo de segredos e não a coloque em `appsettings.json`.

Se sua conexão usar outros valores, informe-os assim:

```powershell
powershell -ExecutionPolicy Bypass -File .\Configurar-MySql.ps1 -Servidor localhost -Porta 3306 -Banco clickclub_dev -Usuario clickclub
```

User Secrets é uma configuração local de desenvolvimento, não um cofre
criptografado. Não execute `dotnet user-secrets list` durante gravações de tela
ou compartilhamentos de terminal: ele exibe os valores. Guarde também a chave
`Credenciais:Chave`, pois trocá-la invalida as carteirinhas já assinadas.

## 3. Iniciar a API

No mesmo terminal PowerShell:

```powershell
cd .\api
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --urls http://localhost:5080
```

Na primeira execução em Development, a API cria as tabelas pela migração
incluída. Em um banco vazio, insere as contas de demonstração uma única vez.
Nas próximas execuções, carrega os dados existentes. Deixe o terminal aberto.

Abra http://localhost:5080/api/saude no navegador. O resultado esperado é:

```json
{"status":"ok","banco":"MySQL","persistencia":"permanente"}
```

Essa URL mostra os dados de saúde da API. Para a interface visual, execute
`Iniciar-ClickClub.ps1` e abra http://localhost:5081. A documentação interativa
da API está disponível quando habilitada na configuração de desenvolvimento.

## 4. Conferir os dados no Workbench

Atualize **Schemas**, expanda `clickclub_dev` e execute:

```sql
USE clickclub_dev;
SHOW TABLES;
SELECT Id, Nome, Login FROM usuarios;
SELECT Id, Nome, Sigla FROM clubes;
SELECT Id, Matricula, Categoria FROM associacoes;
SELECT * FROM __EFMigrationsHistory;
```

Não consulte/exporte `SenhaHash` e `sessoes` para compartilhar resultados.
Para testar a API no Windows, abra outro PowerShell na pasta `ClickClub`:

```powershell
powershell -ExecutionPolicy Bypass -File .\testes\smoke.ps1
powershell -ExecutionPolicy Bypass -File .\testes\cpf.ps1
```

Os scripts antigos que criam registros com nomes fixos podem encontrar
duplicatas ao serem repetidos: agora reiniciar a API **preserva** os dados.
Use apenas dados fictícios. As contas de demonstração (`admin`, `titular`,
`presidente` e demais perfis) mantêm a senha de teste `Teste@123`.

## 5. Verificar a persistência

Depois de alterar um cadastro ou criar uma reserva pela API, encerre com
Ctrl+C, inicie novamente e consulte o registro. Ele deve continuar presente.
Os IDs e as contas também permanecem. Sessões duram até expirar (oito horas)
ou sair; carteirinhas duram até sua validade/revogação e precisam da mesma
chave de assinatura entre execuções.

## Se não conectar

| Mensagem/situação | O que conferir |
|---|---|
| `dotnet` não reconhecido | Instale o SDK .NET 8 e reabra o terminal. |
| Configure `ConnectionStrings:ClickClub` | Execute o script e defina `ASPNETCORE_ENVIRONMENT=Development` antes de iniciar. |
| Configure `Credenciais:Chave` | Execute o script de configuração; ele gera a chave na primeira vez. |
| `Unable to connect` / conexão recusada | Confira se o serviço MySQL está ligado e se hostname/porta são os mesmos da conexão do Workbench. |
| `Access denied` | Confira usuário, senha, host autorizado e permissões sobre `clickclub_dev`; rode o script novamente. |
| `Unknown database` | Execute `001_criar_banco.sql` no mesmo servidor da conexão usada pela API. |
| Tabelas já existem e migração falha | Use um banco separado e vazio. Não apague suas tabelas para tentar corrigir. |
| HTTP 503 | A operação não conseguiu confirmar a gravação no MySQL; verifique a disponibilidade do servidor antes de tentar novamente. |

## Como a adaptação foi feita

- `api/Persistencia/ClickClubDbContext.cs`: tabelas, chaves estrangeiras,
  índices únicos de login/CPF/matrícula/competência e conversões de dados.
- `api/Persistencia/Migrations`: migração inicial versionada.
- `api/Persistencia/Store.MySql.cs`: carrega as coleções existentes do banco e
  identifica inclusões, alterações e exclusões. O `Store` é criado por requisição.
- `api/Persistencia/PersistenciaMiddleware.cs`: transação e trava no MySQL
  para proteger as verificações atuais de reservas/financeiro. A confirmação
  acontece antes de enviar a resposta de sucesso.
- Datas com horário são guardadas em UTC; os fusos das regras continuam nos clubes.
- Coleções internas pequenas (papéis, atribuições, dependentes, interesses,
  respostas e convidados) usam colunas JSON nesta fase. As entidades principais
  têm tabelas próprias. Ainda não é o modelo relacional completo do planejamento.

Este adaptador preserva as regras atuais e é adequado à demonstração acadêmica
com poucos dados. Ele carrega as tabelas por requisição e serializa as operações
por banco: não foi feito para carga alta. A evolução indicada é consultar apenas
os registros necessários por serviço e normalizar as coleções internas.

Em Development, há criação automática e contas fictícias. Fora dele, esses
recursos ficam desligados por padrão. Uma implantação real precisa de um
processo próprio de migração, identidade administrativa inicial, gestão de
segredos/HTTPS, auditoria de segurança e remoção das contas de teste.
Não use os scripts de demonstração para um banco de produção.

`banco/002_estrutura_gerada.sql` é uma referência SQL gerada pela migração para
revisão no Workbench. Não é necessário executá-lo se a API aplicar a migração.
Não utilize `EnsureCreated` junto com as migrações e não apague o banco para
atualizar o modelo. Guarde backups antes de futuras alterações de schema.
