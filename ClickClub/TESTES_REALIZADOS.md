# Testes do ClickClub

## Mobile MAUI para Windows — 19/11/2026

- `ClickClub.sln` compilada no Windows com .NET SDK 10.0.302, com zero erros
  e zero avisos.
- Janela aberta pelo `dotnet run`. O WebView exibiu o login com estilos locais
  e texto em português. O formulário de adesão carregou os clubes da API.
- 14 cenários automatizados do cliente com SQLite real e HTTP simulado.
- 14 cenários da suíte HTTP, incluindo reinício da API, em um MySQL isolado
  para testes.
- `compileall` e `git diff --check` concluídos. Evidências em `tmp_tests`.

## Registro da edição Android — 11/11/2026

> Registro histórico da edição Android. Para executar a edição atual do
> mobile, consulte [MAUI para Windows](mobile/README.md).

## Compilações observadas

Ambiente Linux, SDK .NET 8.0.428. API, Web MVC, Mobile.Core, Mobile.UI,
prévia mobile e aplicativo Android: **zero erros e zero avisos** nas
compilações finais. Android: MAUI 8.0.100, SDK Android 34.0.154,
plataforma/build-tools 34, JDK 17.0.21, alvo `net8.0-android`, configuração
Debug, arquitetura ARM64. Os caminhos locais de ferramentas não são
necessários no computador do usuário: use os SDKs instalados e os guias.

`mobile/apk/ClickClub.apk` foi gerado com as bibliotecas incluídas no APK.
Manifesto conferido: pacote `br.edu.clickclub.mobile`, versão 1.0.0,
Android mínimo API24, alvo API34. Assinatura verificada com `apksigner`:
**válida pelos esquemas v2 e v3**. Conferida a presença das bibliotecas do
aplicativo, SQLite, interface compartilhada e CSS. Checksum ao lado do APK.

**Não houve instalação em telefone físico nem execução em emulador neste
ambiente.** Compilar e verificar assinatura não substitui esse ensaio.

## API com MySQL real

Servidor MySQL 8.0 isolado e dados fictícios. A suíte
`testes/integracao_mysql.py` concluiu **14 grupos de verificação**, incluindo:

- Aplicação das duas migrações, população inicial e saúde do banco.
- Login, autorização, CPF normalizado/inválido/duplicado e vinculação familiar.
- Edição de cadastro/categoria e isolamento de funcionário por unidade.
- Titular não consome autorização emitida para dependente.
- Duas reservas concorrentes: uma aceita, outra recusada; sem sobreposição.
- Transferência financeira com 10% único e repetição sem duplicar cobrança.
- Cancelamento ajustando fatura/itens, turmas, espera e promoção.
- Eventos, visitantes, inscrições e validação de credencial na portaria.
- Atraso de 10 dias sem bloqueio;11 dias bloqueiam o grupo; quitação regulariza.
- Avisos, chamados/respostas, adesão pública e aprovação.
- Reinício real preserva usuários/IDs, sessões, CPF, convidados, reservas,
  auditoria e credenciais; população não duplica. Logout revoga a sessão.

O banco de teste descartável foi removido após as verificações. Nenhum
servidor, senha ou arquivo do computador do usuário foi acessado.

## Web MVC e início coordenado

Teste por HTTP com cookies reais e HTML renderizado, API/MySQL iniciados:

- **82 navegações** pelos menus visíveis dos sete perfis, sem erros HTTP/API
  nas páginas: admin 18, presidente 15, tesoureiro 9, funcionário 11, porteiro 8,
  titular 11 e dependente 10.
- Formulário sem token antifalsificação recusado com HTTP 400.
- Publicação real de aviso por formulário MVC autenticado, exibindo o dado
  persistido pela API.
- Execução real de `Iniciar-ClickClub.ps1` no PowerShell 7.4/Linux:
  compilou/iniciou API e Web, conferiu saúde, e encerrou ambos corretamente.
- `Criar-Solucao.ps1` executado, gerando as duas soluções incluídas.

Não houve automação de navegador interativo nem teste no Windows. A revisão
HTTP não comprova todos os estados visuais, câmera ou todos os formulários.

## Mobile, SQLite e offline

`dotnet run --project mobile/Tests/ClickClub.Mobile.Tests.csproj` concluiu
**14 cenários** com SQLite real e HTTP controlado, sem precisar de Android:

- Login/perfil e armazenamento abstrato de segredo.
- QR gerado e carteirinha válida online.
- Avisos e carteirinha recuperados do SQLite com rede indisponível.
- Dados pessoais de perfil não entram no cache; SQLite sem senha/token de sessão.
- Sessão expirada de 8 horas preserva carteirinha válida de 24 horas.
- Alterações online recusadas após expirar a sessão.
- Carteirinha vencida identificada como expirada offline.
- Troca de servidor apaga sessão/cache; URL com caminho e desvio de host recusados.

Prévia: inicialização real em Production na porta 5082, HTML de login
renderizado e CSS compartilhado respondendo HTTP 200. A interface compartilhada
compilou sem erros. O armazenamento da prévia não é o SecureStorage do Android.

## Ensaio ainda necessário no destino

Instalação/execução física do APK, SecureStorage do aparelho, retomada de
WebView, rede Wi-Fi/firewall, acessibilidade visual e ensaio completo no
Windows/Workbench do apresentador. Não foram executados todos os limites
T01–T41, testes de carga distribuída, recuperação após falha de rede durante
commit ou migração de um banco legado real. Ver `PENDENCIAS.md`.

---

## Histórico da revisão anterior com MySQL

Os registros abaixo pertencem à revisão que antecedeu as interfaces.
As evidências atuais da entrega completa estão nas seções acima.

# Testes — revisão MySQL de 11/11/2026

Compilação com SDK .NET 8.0.428: **zero erros e zero avisos**. Integração
verificada em Linux com servidor MySQL 8.0 isolado, EF Core 8.0.34 e Pomelo
8.0.3, por chamadas HTTP reais à API. Dados exclusivamente fictícios.

Resultados observados nesta revisão:

- Migração inicial cria as tabelas e o histórico de migração; população de
  demonstração cria as contas uma única vez em banco vazio.
- Login, sessão persistida, recusa de senha inválida, autorização por perfil
  e logout com revogação da sessão.
- Vinculação de dependente; CPF válido normalizado, inválido e duplicado
  recusados; dados anteriores preservados após atualização inválida.
- Atualização de clube existente, categoria com diferença de maiúsculas e
  cadastro de atividade.
- Duas reservas HTTP simultâneas para o mesmo período: uma confirmação e
  uma recusa. Não foi realizado teste distribuído com várias instâncias.
- Transferência de taxa para fatura com 10% de acréscimo; repetição sem
  duplicar itens; cancelamento ajusta o valor e exclui os itens transferidos.
- Turmas, horários de aulas, inscrição confirmada/espera, cancelamento e
  promoção manual; substituição de aula com chave composta existente.
- Evento público, visitante, inscrição em evento e validação de portaria;
  emissão e verificação de credencial assinada.
- Avisos, chamados, resposta e alteração de status; adesão pública com
  conta de candidato e aprovação criando associação.
- Reinício real do processo preserva IDs, usuários, sessões, dados pessoais,
  reservas, convidados, respostas de chamados e auditoria. A mesma chave
  configurada permite verificar a credencial emitida antes do reinício.
- Falha de gravação provocada por restrição CHECK temporária no banco de
  teste: HTTP 503 e rollback de fatura, reserva, itens e auditoria, sem resposta
  de sucesso antecipada. A restrição foi removida ao terminar o cenário.
- Cancelamento depois de quitação gera crédito; aplicação reduz fatura e
  permanece marcada após reiniciar, impedindo aplicação duplicada.

Script `Configurar-MySql.ps1`: sintaxe e fluxo verificados em PowerShell
7.4.13/Linux com substituto controlado do comando de User Secrets. Verificados
JSON por stdin, senha fictícia com acento/ponto e vírgula/aspas e preservação
da chave de assinatura ao repetir a configuração. Isso **não equivale** à
execução no Windows nem à conexão com o MySQL do usuário.

Não foram testados: ambiente local do usuário, Windows PowerShell 5.1,
execução dos scripts antigos no Windows, Web/Mobile, carga alta, falhas de
rede durante o commit, toda a matriz funcional do planejamento e integração
com schemas legados. Os scripts originais podem exigir banco de teste limpo,
pois seus registros agora persistem após o reinício.

## Histórico informado no pacote original

O texto abaixo veio no ZIP de origem e descreve a versão em memória. Não
deve ser confundido com testes novos desta integração MySQL.

A API foi compilada em ambiente Linux com SDK .NET 8. Foram realizados testes HTTP automáticos na aplicação iniciada localmente, com dados fictícios em memória. Resultados observados:

- Saúde, login e recusa da análise de adesões por titular.
- Associação de dependente; emissão de credencial e validação online de credencial válida e inválida.
- Reserva de espaço, conflito de horário e duas tentativas simultâneas com apenas uma confirmação.
- Fatura de R$ 150, locação fictícia de R$ 100, prévia de transferência com R$ 10 de acréscimo, transferência repetida sem duplicação e cancelamento ajustando fatura aberta.
- Turma com uma vaga, lista de espera e promoção manual.
- Proteção de dados pessoais contra outro associado; aprovação de adesão e criação de associação.
- Presidente de um clube impedido de criar espaço e consultar relatório financeiro de outro clube.
- Mensalidade atrasada em 10 dias sem bloqueio; 11 dias bloqueando dependente e acesso; quitação simulada retirando o bloqueio.
- Cadastro de visitante para evento público, com identificação correta autorizada e identificação divergente negada pela portaria.
- Autoinscrição com conta de candidato, consulta do pedido pendente, recusa de carteirinha antes da aprovação e emissão depois da aprovação.

Os testes não abrangem todos os 41 cenários da documentação. Não houve teste no Windows desta versão, nem de interface, Android, MySQL, cargas distribuídas, QR em imagem ou todos os limites de data e horário. Os scripts PowerShell incluídos são diferentes da suíte executada no Linux e devem ser testados no computador de destino.
