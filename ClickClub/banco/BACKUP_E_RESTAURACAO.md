# Backup e restauração do banco local

Use o MySQL Workbench conectado ao servidor correto. Um backup do banco não
substitui a cópia da chave `Credenciais:Chave` configurada no .NET User Secrets.
Sem a mesma chave, carteirinhas emitidas anteriormente não serão verificáveis.

## Pelo Workbench

1. Abra **Server → Data Export** e selecione somente `clickclub_dev`.
2. Selecione estrutura e dados, **Export to Self-Contained File** e escolha
   um arquivo privado, por exemplo `clickclub_backup.sql`.
3. Inicie a exportação e confira se terminou sem erro. Não envie o backup
   para terceiros: ele contém dados pessoais e sessões.
4. Para verificar a recuperação, crie **outro schema**, `clickclub_restore_test`.
5. Abra **Server → Data Import**, escolha o arquivo e selecione o schema de
   teste como destino. Confira antes se o arquivo contém comandos `USE` ou
   `CREATE DATABASE` apontando para o banco original; não importe por cima dele.
6. Confira contagens e integridade das tabelas no schema restaurado.

## Pela linha de comando

Os utilitários `mysqldump` e `mysql` precisam estar instalados e no PATH.
O parâmetro `-p` pede a senha sem colocá-la na linha de comando.

```powershell
mysqldump -h localhost -P 3306 -u clickclub -p --single-transaction --no-tablespaces --result-file=clickclub_backup.sql clickclub_dev
```

Para restaurar, use uma conta autorizada a criar o schema de teste, crie
`clickclub_restore_test` no Workbench e confira o SQL de backup. No cliente
`mysql`, já conectado ao schema de teste, execute `source`:

```text
mysql -h localhost -P 3306 -u USUARIO_DE_RESTAURACAO -p clickclub_restore_test
source C:/caminho/privado/clickclub_backup.sql
SHOW TABLES;
SELECT COUNT(*) FROM usuarios;
SELECT COUNT(*) FROM reservas;
```

Não execute `DROP DATABASE`, `docker compose down -v` ou importações sobre
seu banco de apresentação para repetir testes. O volume do Compose é
persistente; `docker compose stop` apenas interrompe o serviço.

Este procedimento está documentado para repetição local. Não representa
evidência de que um backup do banco do usuário tenha sido criado ou restaurado.
