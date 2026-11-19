# Limites técnicos e verificações da entrega

O projeto é uma entrega acadêmica com dados, pagamentos e controle de acesso
simulados. O código e as evidências de execução estão descritos em
`MATRIZ_ENTREGA.md` e `TESTES_REALIZADOS.md`. A documentação funcional original
preserva decisões históricas; ela não substitui o registro de testes atual.

## Antes da apresentação no computador do usuário

- Configurar a conexão MySQL local com o script e conferir o servidor/porta.
- Repetir o início da API e Web no Windows e ensaiar os perfis do roteiro.
- Abrir o app MAUI no Windows e verificar a sessão, o cache SQLite e a validade
  da carteirinha sem conexão com a API de teste.
- Confirmar as portas locais e os pré-requisitos do
  [Mobile para Windows](mobile/README.md).
- Fazer backup do banco e preservar a chave de assinatura de carteirinhas.

Essas verificações devem ser repetidas no notebook da apresentação. Os
resultados observados no Windows estão em [TESTES_REALIZADOS.md](TESTES_REALIZADOS.md).

## Escala e implantação

- O adaptador de persistência preserva as coleções e contratos da API original.
  Carrega tabelas por requisição e usa uma trava no MySQL por banco. A evolução
  para carga alta exige consultas específicas e concorrência por recurso.
- Papéis, atribuições, dependentes e outras coleções pequenas são mantidos em
  colunas JSON. As entidades principais têm tabelas próprias, mas o modelo
  relacional completo do planejamento exige evolução adicional.
- Contas fictícias, HTTP local e migração automática são de desenvolvimento.
  Produção exige configuração de HTTPS, gestão de identidade/segredos,
  backups monitorados, revisão de privacidade/retenção e auditoria de segurança.
- O SQLite é um cache limitado por conta/servidor. Escritas de reserva,
  financeiro e autorização de portaria exigem a API online; não existe fila
  de transações offline nem sincronização irrestrita de dados pessoais.

## Fora do escopo aprovado

Push remoto, aplicativo iOS, torneios com chaves/resultados, pagamentos reais,
catracas físicas e validação offline da portaria não fazem parte da primeira
entrega. Uma imagem QR em cache não equivale a autorização de entrada.

Os limites acima não são uma declaração de execução integral de todos os
cenários T01–T41. Consulte quais fluxos foram efetivamente verificados em
`TESTES_REALIZADOS.md` e repita o ensaio com o aplicativo MAUI para Windows.
