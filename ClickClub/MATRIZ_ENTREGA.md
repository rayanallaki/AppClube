# Matriz de entrega

Esta matriz aponta onde os fluxos foram implementados. **Implementação não
significa teste em todos os dispositivos ou execução de todos os limites**.
Consulte `TESTES_REALIZADOS.md` para as evidências efetivamente observadas.
Os nomes de tela podem ser agrupados nos menus de gestão e serviços.

| Requisitos | Componente responsável | Fluxo |
|---|---|---|
| RF01, RF09 | API + Web + Mobile | Login, perfis e atribuições por unidade; clientes respeitam resposta da API. |
| RF02, RF03, RF10–12, RF28 | API + Web + Mobile | Espaços, horários, manutenção, agenda, reservas, convidados e cancelamento. |
| RF04, RF20, RF23 | API + Web + Mobile/SQLite | Carteirinha distinta da sessão, QR, validade/cache e conferência online da portaria. |
| RF05 | API | Bloqueio por mensalidade vencida além dos dez dias e regularização simulada. |
| RF06–08, RF25, RF29 | API + Web + Mobile | Unidades, pessoas/dados pessoais, vínculos, categorias e dependentes. |
| RF13, RF14, RF24 | API + Web + Mobile | Mensalidade e itens, quitação, transferência com acréscimo e crédito simulado. |
| RF15, RF16 | API + Web + Mobile | Atividades, turmas/aulas, inscrições e lista de espera de turmas. |
| RF17, RF18, RF27 | API + Web + Mobile | Avisos, ouvidoria com respostas/status e eventos sociais. |
| RF21, RF22 | API + Web | Relatórios básicos, painel e auditoria das operações. |
| RF26 | API + Web + Mobile | Pedido público com candidato, consulta e decisão por administrador/presidente. |
| RNF01–06 | API/MySQL + Mobile/SQLite | Separação de acesso ao banco, transações, armazenamento e validade. |
| RNF07, RNF11 | Web + Mobile + raiz | Interfaces em português, estrutura por projeto e guias. |
| RNF10 | banco | Procedimento de backup/restauração em schema separado. |
| RF19 | Fora da primeira entrega | Push remoto adiado no documento aprovado. |

## Critérios que precisam do ensaio local

- Android físico: instalação, câmera quando utilizada, permissões,
  SecureStorage, cache SQLite e exibição offline da credencial.
- Windows/Workbench do usuário: conexão local, credenciais, firewall e
  inicialização dos projetos nos SDKs instalados.
- Toda a matriz T01–T41, carga alta e falhas de rede durante confirmação de
  transação: não são inferidas a partir de uma compilação sem erros.

A geração de imagem QR e a leitura/entrada do código na portaria continuam
sujeitas ao servidor online. Nenhuma tela autoriza catraca ou pagamento real.
