CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `atividades` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Descricao` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Tipo` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_atividades` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `categorias` (
        `name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Ativa` tinyint(1) NOT NULL,
        `ValorMensal` decimal(18,2) NULL,
        `LimiteDependentes` int NULL,
        `Beneficios` longtext CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_categorias` PRIMARY KEY (`name`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `clubes` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Sigla` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Endereco` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Bairro` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Municipio` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Uf` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Telefone` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Email` longtext CHARACTER SET utf8mb4 NOT NULL,
        `FusoHorario` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Ativo` tinyint(1) NOT NULL,
        CONSTRAINT `PK_clubes` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `usuarios` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Login` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `SenhaHash` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Ativo` tinyint(1) NOT NULL,
        `Papeis` json NOT NULL,
        `Clubes` json NOT NULL,
        `Atribuicoes` json NOT NULL,
        `cpf` varchar(11) CHARACTER SET utf8mb4 NULL,
        `Dados_DataNascimento` date NULL,
        `Dados_Endereco` longtext CHARACTER SET utf8mb4 NULL,
        `Dados_Bairro` longtext CHARACTER SET utf8mb4 NULL,
        `Dados_Cidade` longtext CHARACTER SET utf8mb4 NULL,
        `Dados_Uf` longtext CHARACTER SET utf8mb4 NULL,
        `Dados_Sexo` longtext CHARACTER SET utf8mb4 NULL,
        `Dados_EstadoCivil` longtext CHARACTER SET utf8mb4 NULL,
        `Dados_Telefone` longtext CHARACTER SET utf8mb4 NULL,
        `Dados_Email` longtext CHARACTER SET utf8mb4 NULL,
        `Dados_Interesses` json NOT NULL,
        CONSTRAINT `PK_usuarios` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `avisos` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClubeId` char(36) COLLATE ascii_general_ci NULL,
        `Titulo` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Corpo` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_avisos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_avisos_clubes_ClubeId` FOREIGN KEY (`ClubeId`) REFERENCES `clubes` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `espacos` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClubeId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Tipo` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Capacidade` int NOT NULL,
        `Taxa` decimal(18,2) NOT NULL,
        `Ativo` tinyint(1) NOT NULL,
        CONSTRAINT `PK_espacos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_espacos_clubes_ClubeId` FOREIGN KEY (`ClubeId`) REFERENCES `clubes` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `eventos` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClubeId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Descricao` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Inicio` datetime(6) NOT NULL,
        `Fim` datetime(6) NOT NULL,
        `PublicoAlvo` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Capacidade` int NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_eventos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_eventos_clubes_ClubeId` FOREIGN KEY (`ClubeId`) REFERENCES `clubes` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `turmas` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClubeId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Inicio` datetime(6) NOT NULL,
        `Fim` datetime(6) NOT NULL,
        `Capacidade` int NOT NULL,
        `Ativa` tinyint(1) NOT NULL,
        CONSTRAINT `PK_turmas` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_turmas_clubes_ClubeId` FOREIGN KEY (`ClubeId`) REFERENCES `clubes` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `acessos` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClubeId` char(36) COLLATE ascii_general_ci NOT NULL,
        `UsuarioId` char(36) COLLATE ascii_general_ci NULL,
        `PorteiroId` char(36) COLLATE ascii_general_ci NOT NULL,
        `DataHora` datetime(6) NOT NULL,
        `Permitido` tinyint(1) NOT NULL,
        `Motivo` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_acessos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_acessos_clubes_ClubeId` FOREIGN KEY (`ClubeId`) REFERENCES `clubes` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_acessos_usuarios_PorteiroId` FOREIGN KEY (`PorteiroId`) REFERENCES `usuarios` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_acessos_usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `associacoes` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UsuarioTitularId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClubeOrigemId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Matricula` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Categoria` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `TipoVinculo` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Ativa` tinyint(1) NOT NULL,
        `Dependentes` json NOT NULL,
        CONSTRAINT `PK_associacoes` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_associacoes_categorias_Categoria` FOREIGN KEY (`Categoria`) REFERENCES `categorias` (`name`) ON DELETE RESTRICT,
        CONSTRAINT `FK_associacoes_clubes_ClubeOrigemId` FOREIGN KEY (`ClubeOrigemId`) REFERENCES `clubes` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_associacoes_usuarios_UsuarioTitularId` FOREIGN KEY (`UsuarioTitularId`) REFERENCES `usuarios` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `auditoria` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UsuarioId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Acao` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Entidade` longtext CHARACTER SET utf8mb4 NOT NULL,
        `EntidadeId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClubeId` char(36) COLLATE ascii_general_ci NULL,
        `DataHora` datetime(6) NOT NULL,
        CONSTRAINT `PK_auditoria` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_auditoria_usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `chamados` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UsuarioId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClubeId` char(36) COLLATE ascii_general_ci NULL,
        `Tipo` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Assunto` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Descricao` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Respostas` json NOT NULL,
        CONSTRAINT `PK_chamados` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_chamados_clubes_ClubeId` FOREIGN KEY (`ClubeId`) REFERENCES `clubes` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_chamados_usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `sessoes` (
        `Token` varchar(64) COLLATE ascii_bin NOT NULL,
        `UserId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Expires` datetime(6) NOT NULL,
        CONSTRAINT `PK_sessoes` PRIMARY KEY (`Token`),
        CONSTRAINT `FK_sessoes_usuarios_UserId` FOREIGN KEY (`UserId`) REFERENCES `usuarios` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `horarios_espacos` (
        `EspacoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `DiaSemana` int NOT NULL,
        `Inicio` time(6) NOT NULL,
        `Fim` time(6) NOT NULL,
        CONSTRAINT `PK_horarios_espacos` PRIMARY KEY (`EspacoId`, `DiaSemana`, `Inicio`),
        CONSTRAINT `FK_horarios_espacos_espacos_EspacoId` FOREIGN KEY (`EspacoId`) REFERENCES `espacos` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `interdicoes` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EspacoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Inicio` datetime(6) NOT NULL,
        `Fim` datetime(6) NOT NULL,
        `Motivo` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_interdicoes` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_interdicoes_espacos_EspacoId` FOREIGN KEY (`EspacoId`) REFERENCES `espacos` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `inscricoes_eventos` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EventoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `UsuarioId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_inscricoes_eventos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_inscricoes_eventos_eventos_EventoId` FOREIGN KEY (`EventoId`) REFERENCES `eventos` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_inscricoes_eventos_usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `visitantes_eventos` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EventoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Identificacao` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_visitantes_eventos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_visitantes_eventos_eventos_EventoId` FOREIGN KEY (`EventoId`) REFERENCES `eventos` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `aulas` (
        `TurmaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Inicio` datetime(6) NOT NULL,
        `Fim` datetime(6) NOT NULL,
        CONSTRAINT `PK_aulas` PRIMARY KEY (`TurmaId`, `Inicio`, `Fim`),
        CONSTRAINT `FK_aulas_turmas_TurmaId` FOREIGN KEY (`TurmaId`) REFERENCES `turmas` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `inscricoes_turmas` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `TurmaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `UsuarioId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `InscritaEm` datetime(6) NOT NULL,
        `PromovidoPor` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_inscricoes_turmas` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_inscricoes_turmas_turmas_TurmaId` FOREIGN KEY (`TurmaId`) REFERENCES `turmas` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_inscricoes_turmas_usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `adesoes` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClubeId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Nome` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Email` longtext CHARACTER SET utf8mb4 NOT NULL,
        `TipoVinculo` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `SolicitadaEm` datetime(6) NOT NULL,
        `DecididaEm` datetime(6) NULL,
        `DecididaPor` char(36) COLLATE ascii_general_ci NULL,
        `Motivo` longtext CHARACTER SET utf8mb4 NULL,
        `AssociacaoId` char(36) COLLATE ascii_general_ci NULL,
        `UsuarioId` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_adesoes` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_adesoes_associacoes_AssociacaoId` FOREIGN KEY (`AssociacaoId`) REFERENCES `associacoes` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_adesoes_clubes_ClubeId` FOREIGN KEY (`ClubeId`) REFERENCES `clubes` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_adesoes_usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `autorizacoes_dependentes` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `AssociacaoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `DependenteId` char(36) COLLATE ascii_general_ci NOT NULL,
        `EspacoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Usada` tinyint(1) NOT NULL,
        CONSTRAINT `PK_autorizacoes_dependentes` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_autorizacoes_dependentes_associacoes_AssociacaoId` FOREIGN KEY (`AssociacaoId`) REFERENCES `associacoes` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_autorizacoes_dependentes_espacos_EspacoId` FOREIGN KEY (`EspacoId`) REFERENCES `espacos` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_autorizacoes_dependentes_usuarios_DependenteId` FOREIGN KEY (`DependenteId`) REFERENCES `usuarios` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `credenciais` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `UsuarioId` char(36) COLLATE ascii_general_ci NOT NULL,
        `AssociacaoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `EmitidaEm` datetime(6) NOT NULL,
        `ValidaAte` datetime(6) NOT NULL,
        `Revogada` tinyint(1) NOT NULL,
        CONSTRAINT `PK_credenciais` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_credenciais_associacoes_AssociacaoId` FOREIGN KEY (`AssociacaoId`) REFERENCES `associacoes` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_credenciais_usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `faturas` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `AssociacaoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Competencia` varchar(7) CHARACTER SET utf8mb4 NOT NULL,
        `Vencimento` date NOT NULL,
        `Valor` decimal(18,2) NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_faturas` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_faturas_associacoes_AssociacaoId` FOREIGN KEY (`AssociacaoId`) REFERENCES `associacoes` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `reservas` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EspacoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `AssociacaoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Inicio` datetime(6) NOT NULL,
        `Fim` datetime(6) NOT NULL,
        `Finalidade` longtext CHARACTER SET utf8mb4 NULL,
        `Valor` decimal(18,2) NOT NULL,
        `MesLocal` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `StatusTaxa` longtext CHARACTER SET utf8mb4 NOT NULL,
        `ResponsavelId` char(36) COLLATE ascii_general_ci NOT NULL,
        CONSTRAINT `PK_reservas` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_reservas_associacoes_AssociacaoId` FOREIGN KEY (`AssociacaoId`) REFERENCES `associacoes` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_reservas_espacos_EspacoId` FOREIGN KEY (`EspacoId`) REFERENCES `espacos` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_reservas_usuarios_ResponsavelId` FOREIGN KEY (`ResponsavelId`) REFERENCES `usuarios` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `convidados_reservas` (
        `ReservaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Convidados` json NOT NULL,
        CONSTRAINT `PK_convidados_reservas` PRIMARY KEY (`ReservaId`),
        CONSTRAINT `FK_convidados_reservas_reservas_ReservaId` FOREIGN KEY (`ReservaId`) REFERENCES `reservas` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `creditos` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `AssociacaoId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ReservaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Valor` decimal(18,2) NOT NULL,
        `Descricao` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_creditos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_creditos_associacoes_AssociacaoId` FOREIGN KEY (`AssociacaoId`) REFERENCES `associacoes` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_creditos_reservas_ReservaId` FOREIGN KEY (`ReservaId`) REFERENCES `reservas` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `itens_faturas` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `FaturaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `ReservaId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Descricao` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Valor` decimal(18,2) NOT NULL,
        CONSTRAINT `PK_itens_faturas` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_itens_faturas_faturas_FaturaId` FOREIGN KEY (`FaturaId`) REFERENCES `faturas` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_itens_faturas_reservas_ReservaId` FOREIGN KEY (`ReservaId`) REFERENCES `reservas` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE TABLE `creditos_aplicados` (
        `CreditoId` char(36) COLLATE ascii_general_ci NOT NULL,
        CONSTRAINT `PK_creditos_aplicados` PRIMARY KEY (`CreditoId`),
        CONSTRAINT `FK_creditos_aplicados_creditos_CreditoId` FOREIGN KEY (`CreditoId`) REFERENCES `creditos` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_acessos_ClubeId` ON `acessos` (`ClubeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_acessos_PorteiroId` ON `acessos` (`PorteiroId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_acessos_UsuarioId` ON `acessos` (`UsuarioId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_adesoes_AssociacaoId` ON `adesoes` (`AssociacaoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_adesoes_ClubeId` ON `adesoes` (`ClubeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_adesoes_UsuarioId` ON `adesoes` (`UsuarioId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_associacoes_Categoria` ON `associacoes` (`Categoria`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_associacoes_ClubeOrigemId` ON `associacoes` (`ClubeOrigemId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE UNIQUE INDEX `IX_associacoes_Matricula` ON `associacoes` (`Matricula`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_associacoes_UsuarioTitularId` ON `associacoes` (`UsuarioTitularId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_auditoria_UsuarioId` ON `auditoria` (`UsuarioId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_autorizacoes_dependentes_AssociacaoId` ON `autorizacoes_dependentes` (`AssociacaoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_autorizacoes_dependentes_DependenteId` ON `autorizacoes_dependentes` (`DependenteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_autorizacoes_dependentes_EspacoId` ON `autorizacoes_dependentes` (`EspacoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_avisos_ClubeId` ON `avisos` (`ClubeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_chamados_ClubeId` ON `chamados` (`ClubeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_chamados_UsuarioId` ON `chamados` (`UsuarioId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE UNIQUE INDEX `IX_clubes_Sigla` ON `clubes` (`Sigla`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_credenciais_AssociacaoId` ON `credenciais` (`AssociacaoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_credenciais_UsuarioId` ON `credenciais` (`UsuarioId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_creditos_AssociacaoId` ON `creditos` (`AssociacaoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_creditos_ReservaId` ON `creditos` (`ReservaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_espacos_ClubeId` ON `espacos` (`ClubeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_eventos_ClubeId` ON `eventos` (`ClubeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE UNIQUE INDEX `IX_faturas_AssociacaoId_Competencia` ON `faturas` (`AssociacaoId`, `Competencia`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_inscricoes_eventos_EventoId` ON `inscricoes_eventos` (`EventoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_inscricoes_eventos_UsuarioId` ON `inscricoes_eventos` (`UsuarioId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_inscricoes_turmas_TurmaId` ON `inscricoes_turmas` (`TurmaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_inscricoes_turmas_UsuarioId` ON `inscricoes_turmas` (`UsuarioId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_interdicoes_EspacoId` ON `interdicoes` (`EspacoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_itens_faturas_FaturaId` ON `itens_faturas` (`FaturaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_itens_faturas_ReservaId` ON `itens_faturas` (`ReservaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_reservas_AssociacaoId` ON `reservas` (`AssociacaoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_reservas_EspacoId_Inicio_Fim` ON `reservas` (`EspacoId`, `Inicio`, `Fim`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_reservas_ResponsavelId` ON `reservas` (`ResponsavelId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_sessoes_UserId` ON `sessoes` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_turmas_ClubeId` ON `turmas` (`ClubeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE UNIQUE INDEX `IX_usuarios_cpf` ON `usuarios` (`cpf`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE UNIQUE INDEX `IX_usuarios_Login` ON `usuarios` (`Login`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    CREATE INDEX `IX_visitantes_eventos_EventoId` ON `visitantes_eventos` (`EventoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111150633_MySqlInicial') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20261111150633_MySqlInicial', '8.0.34');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111160248_FluxosCompletos') THEN

    ALTER TABLE `turmas` ADD `AtividadeId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111160248_FluxosCompletos') THEN

    ALTER TABLE `turmas` ADD `Descricao` longtext CHARACTER SET utf8mb4 NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111160248_FluxosCompletos') THEN

    ALTER TABLE `turmas` ADD `Instrutor` longtext CHARACTER SET utf8mb4 NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111160248_FluxosCompletos') THEN

    ALTER TABLE `reservas` ADD `CanceladaEm` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111160248_FluxosCompletos') THEN

    ALTER TABLE `reservas` ADD `Multa` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111160248_FluxosCompletos') THEN

    ALTER TABLE `reservas` ADD `Reembolso` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111160248_FluxosCompletos') THEN

    ALTER TABLE `faturas` ADD `ValorBase` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111160248_FluxosCompletos') THEN


                    UPDATE faturas f
                    LEFT JOIN (SELECT FaturaId, SUM(Valor) valorItens FROM itens_faturas GROUP BY FaturaId) i
                      ON i.FaturaId = f.Id
                    SET f.ValorBase = f.Valor - COALESCE(i.valorItens, 0);
                

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111160248_FluxosCompletos') THEN

    CREATE INDEX `IX_turmas_AtividadeId` ON `turmas` (`AtividadeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111160248_FluxosCompletos') THEN

    ALTER TABLE `turmas` ADD CONSTRAINT `FK_turmas_atividades_AtividadeId` FOREIGN KEY (`AtividadeId`) REFERENCES `atividades` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261111160248_FluxosCompletos') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20261111160248_FluxosCompletos', '8.0.34');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

