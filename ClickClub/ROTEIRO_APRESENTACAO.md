# Roteiro de apresentação — ClickClub

Tempo sugerido: 10 a 15 minutos. Faça um ensaio completo no notebook
que usará na apresentação. Mantenha MySQL, API e Web ligados e
use somente dados fictícios. O roteiro descreve o que demonstrar; consulte o
registro de testes para distinguir os cenários já executados neste ambiente.

## Preparação

- Abra `README.md` e execute a configuração do banco e o script de início.
- Confira a Web em `http://localhost:5081` e a saúde da API na porta 5080.
- Abra o Mobile na janela do Windows com `dotnet run` na pasta `mobile`.
  Confira se o servidor cadastrado no app é `http://localhost:5080/`.
- Confira que as categorias têm preço e limites coerentes para o exemplo.
- Não execute repetidamente os testes sobre o banco da apresentação: os
  dados agora permanecem após o reinício.
- Tenha duas janelas ou perfis de navegador para alternar titular e
  administração/portaria. Não exponha o Workbench com a tabela de sessões.

## Contas fictícias

Todas usam a senha de demonstração `Teste@123` em banco inicialmente vazio.

| Conta | O que apresentar |
|---|---|
| `admin` | Administração central, unidades, usuários e auditoria. |
| `presidente` | Gestão e aprovação de adesões da unidade atribuída. |
| `tesoureiro` | Faturas, quitação e relatórios da unidade autorizada. |
| `funcionario` | Espaços, atividades, turmas e atendimento local. |
| `porteiro` | Consulta online de credencial e histórico de acesso. |
| `titular` | Serviços da associação, reservas, financeiro e carteirinha. |
| `dependente` | Serviços permitidos sem acesso ao financeiro do titular. |

Os IDs originais permanecem estáveis: clube
`11111111-1111-1111-1111-111111111111`, associação
`22222222-2222-2222-2222-222222222222` e salão
`33333333-3333-3333-3333-333333333333`. Vincule o dependente à associação
pela administração ou pela área do titular antes de demonstrar seus serviços.

## 1. Explicar a arquitetura — 1 minuto

“A Web e o app conversam com a API. A API confere permissões e regras e
grava no MySQL. O SQLite no aplicativo MAUI guarda apenas o cache necessário para
consulta e a última carteirinha, enquanto a decisão da portaria é online.”

Mostre as três pastas do projeto. Explique que categoria (Familiar,
Individual, Atleta), tipo de vínculo e perfil de acesso são conceitos distintos.

## 2. Administração e adesão — 2 minutos

Entre como `admin`, mostre o painel e as unidades. Cadastre ou atualize um
registro fictício. Pelo app/área pública, envie um pedido de adesão com login
novo como Convidado ou Simpatizante. Mostre seu estado pendente. Na Web,
entre como `presidente` ou `admin`, aprove com matrícula e categoria e volte
ao candidato para mostrar o vínculo criado. A pendência não dá direitos de sócio.

## 3. Reserva sem conflito — 2 minutos

Entre como titular, escolha um espaço e um período futuro com mais de 24
horas de antecedência. Informe finalidade e convidados fictícios. Confirme e
mostre a reserva na agenda. Tente reservar o mesmo espaço/período por outra
sessão: a API deve recusar. Mostre o limite mensal e a política de cancelamento.

## 4. Financeiro simulado — 2 minutos

Crie/prepare uma mensalidade futura elegível e mostre seu valor base. Para
um espaço com taxa de R$ 100, consulte a prévia de transferência com R$ 10
de acréscimo. Confirme uma vez e consulte os itens. Repita a ação para mostrar
que não duplica a cobrança. Cancele no prazo e confira o ajuste na fatura.
Explique que todos os valores e pagamentos são simulados.

## 5. Mobile para Windows e carteirinha — 2 minutos

Mostre o login no app, a carteirinha com QR e o horário de validade. Pare a
API de teste e abra a carteirinha previamente guardada em SQLite.
Mostre que o app diferencia dados em cache e credencial expirada. Na Web,
reinicie a API, entre como porteiro e valide a credencial, exibindo permitido ou
negado e o motivo. A credencial é diferente do token de login.

Não diga que o modo offline libera acesso. Quando a API está indisponível,
a portaria não autoriza automaticamente.

## 6. Turmas, eventos e atendimento — 2 minutos

Cadastre uma turma com uma vaga. Inscreva o titular e um dependente já
vinculado. Mostre a espera, cancele a primeira inscrição e promova o próximo
como funcionário. Mostre um evento social e um chamado de Solicitação ou
Reclamação com resposta e mudança de status.

## 7. Persistência e auditoria — 1 minuto

Mostre um registro recém-criado na Web, encerre e reinicie API/Web e
consulte novamente. No Workbench, consulte somente campos não secretos:

```sql
USE clickclub_dev;
SELECT Id, Nome, Login FROM usuarios;
SELECT Id, Matricula, Categoria FROM associacoes;
SELECT Id, Inicio, Fim, Status FROM reservas;
SELECT Id, Competencia, Valor, Status FROM faturas;
SELECT Acao, Entidade, DataHora FROM auditoria ORDER BY DataHora DESC LIMIT 10;
```

Finalize mostrando o arquivo de testes e as limitações efetivamente
registradas. Funcionalidades futuras, como push remoto, iOS e catraca física,
não fazem parte do escopo acadêmico aprovado.
