-- [Комментарий ИИ-агента]: Явное однократное обновление существующей базы StoreCMS на MySQL.
-- [Комментарий ИИ-агента]: Остановите все экземпляры сайта, создайте резервную копию, выберите целевую БД и выполните этот файл.
-- [Комментарий ИИ-агента]: Требуются права CREATE ROUTINE, ALTER и CREATE. Пароли и legacy-соли не используются.
-- [Комментарий ИИ-агента]: DDL не является транзакционным. После прерывания проверьте и восстановите состояние перед повтором; не очищайте маркер.
DELIMITER $$
CREATE PROCEDURE StoreCMS_UpgradeSecurityV1()
BEGIN
    DECLARE lock_name VARCHAR(64);
    DECLARE lock_acquired INT DEFAULT 0;
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        IF lock_acquired = 1 THEN
            DO RELEASE_LOCK(lock_name);
        END IF;
        RESIGNAL;
    END;
    IF DATABASE() IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Select the existing StoreCMS database first';
    END IF;
    SET lock_name = CONCAT('storecms-security:', LEFT(SHA2(DATABASE(), 256), 40));
    SELECT GET_LOCK(lock_name, 60) INTO lock_acquired;
    IF lock_acquired IS NULL OR lock_acquired <> 1 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Security installation lock unavailable';
    END IF;
    IF (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema=DATABASE()
        AND table_name IN ('Users','UserTypes','ConnectedUsers','AdminPanelLogs')) <> 4 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Existing StoreCMS schema required';
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema=DATABASE() AND table_name='SecurityInstallation')
        OR EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema=DATABASE()
            AND table_name='Users' AND column_name='RequirePasswordChange') THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Upgrade already applied or incomplete; inspect schema without resetting markers';
    END IF;

    -- [Комментарий ИИ-агента]: Проверяем фактический столбец до первого ALTER; тип и длина автоматически не изменяются.
    IF (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=DATABASE()
        AND table_name='Users' AND column_name='Password'
        AND data_type IN ('char','varchar','tinytext','text','mediumtext','longtext')
        AND character_maximum_length >= 84 AND generation_expression='') <> 1 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Users.Password must be a writable character column with capacity >= 84 characters; no schema or data changes made';
    END IF;

    ALTER TABLE `Users` ADD COLUMN `RequirePasswordChange` tinyint(1) NOT NULL DEFAULT 0;
    CREATE TABLE `SecurityInstallation` (
        `ID` int NOT NULL,
        `SchemaVersion` int NOT NULL,
        `InitializedUtc` datetime(6) NOT NULL,
        `LegacyPasswordResetCompleted` tinyint(1) NOT NULL,
        PRIMARY KEY (`ID`)
    ) ENGINE=InnoDB;
    START TRANSACTION;
    INSERT INTO `SecurityInstallation` (`ID`,`SchemaVersion`,`InitializedUtc`,`LegacyPasswordResetCompleted`)
        VALUES (1,1,UTC_TIMESTAMP(6),0);
    COMMIT;
    DO RELEASE_LOCK(lock_name);
END$$
CALL StoreCMS_UpgradeSecurityV1()$$
DROP PROCEDURE StoreCMS_UpgradeSecurityV1$$
DELIMITER ;
