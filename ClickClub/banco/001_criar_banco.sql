-- Execute no MySQL Workbench conectado ao seu servidor local.
-- Use um banco novo. Não execute sobre o schema de outro projeto.
CREATE DATABASE IF NOT EXISTS clickclub_dev
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Crie o usuário na tela Administration > Users and Privileges do Workbench.
-- Sugestão: nome clickclub, host localhost, senha definida por você.
-- Em Schema Privileges, conceda permissões SOMENTE em clickclub_dev.
-- As migrações de desenvolvimento precisam de permissões para criar/alterar
-- tabelas e índices, além de SELECT, INSERT, UPDATE e DELETE.
-- Não é necessário executar 002_estrutura_gerada.sql manualmente: ao iniciar
-- em Development, a API aplica a migração incluída automaticamente.

USE clickclub_dev;

-- Execute este arquivo com uma conta administradora do MySQL.
-- O erro "Access denied ... to database clickclub_dev" indica que o
-- usuario existe, mas nao recebeu privilegios no schema.
-- Se a conta ja existir, mantenha a senha atual e execute os GRANTs abaixo.
-- No erro reportado, a conta autenticada foi clickclub@%. Conceda os
-- privilegios a essa conta sem trocar a senha configurada no User Secrets.
GRANT ALL PRIVILEGES ON clickclub_dev.* TO 'clickclub'@'%';
FLUSH PRIVILEGES;
