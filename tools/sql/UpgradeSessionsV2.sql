-- [Комментарий ИИ-агента]: Выполнять после 2.1, при остановленных экземплярах, с резервной копией выбранной БД. DDL MySQL не откатывается транзакцией.
-- [Комментарий ИИ-агента]: Старые сессии отзываются; таблицы пользователей и маркер установки не изменяются. Повтор на итоговой схеме сохраняет новые сессии.
DELIMITER $$
CREATE PROCEDURE StoreCMS_UpgradeSessionsV2()
main: BEGIN
    DECLARE lock_name VARCHAR(64);
    DECLARE lock_acquired INT DEFAULT 0;
    DECLARE old_index VARCHAR(64);
    DECLARE old_index_count INT;
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        IF lock_acquired = 1 THEN DO RELEASE_LOCK(lock_name); END IF;
        RESIGNAL;
    END;
    IF DATABASE() IS NULL THEN SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT='Select the StoreCMS database first'; END IF;
    SET lock_name=CONCAT('storecms-security:', LEFT(SHA2(DATABASE(),256),40));
    SELECT GET_LOCK(lock_name,60) INTO lock_acquired;
    IF lock_acquired IS NULL OR lock_acquired<>1 THEN SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT='Security installation lock unavailable'; END IF;
    IF (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema=DATABASE() AND table_name IN ('Users','ConnectedUsers','SecurityInstallation') AND engine='InnoDB' AND table_type='BASE TABLE')<>3 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT='InnoDB Users, ConnectedUsers and SecurityInstallation required';
    END IF;
    IF (SELECT COUNT(*) FROM SecurityInstallation WHERE ID=1 AND SchemaVersion=1 AND LegacyPasswordResetCompleted=1)<>1 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT='Complete security upgrade 2.1 first';
    END IF;
    IF (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='Users' AND column_name='Password' AND data_type IN ('char','varchar','tinytext','text','mediumtext','longtext') AND character_maximum_length>=84 AND generation_expression='')<>1 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT='Writable textual Users.Password with capacity >=84 required';
    END IF;
    IF (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='ConnectedUsers' AND column_name IN ('ID','UserID') AND data_type='int' AND is_nullable='NO' AND generation_expression='')<>2
        OR (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='ConnectedUsers' AND column_name='ID' AND column_key='PRI' AND extra='auto_increment')<>1 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT='Expected integer session primary and user keys required';
    END IF;
    IF (SELECT COUNT(*) FROM information_schema.statistics WHERE table_schema=DATABASE() AND table_name='ConnectedUsers' AND column_name='UserID' AND non_unique=0 AND seq_in_index=1 AND sub_part IS NULL AND index_name IN (SELECT index_name FROM information_schema.statistics WHERE table_schema=DATABASE() AND table_name='ConnectedUsers' GROUP BY index_name HAVING COUNT(*)=1))<>1 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT='Unique full UserID index required';
    END IF;
    IF (SELECT COUNT(*) FROM information_schema.key_column_usage WHERE table_schema=DATABASE() AND table_name='ConnectedUsers' AND column_name='UserID' AND referenced_table_name='Users' AND referenced_column_name='ID')<>1
        OR EXISTS (SELECT 1 FROM information_schema.key_column_usage WHERE referenced_table_schema=DATABASE() AND referenced_table_name='ConnectedUsers') THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT='Expected session foreign key required; incoming references are unsupported';
    END IF;
    IF (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='ConnectedUsers')=6
        AND (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='ConnectedUsers' AND column_name='TokenHash' AND data_type='char' AND character_maximum_length=64 AND character_set_name='ascii' AND collation_name='ascii_bin' AND is_nullable='NO' AND generation_expression='')=1
        AND (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='ConnectedUsers' AND column_name='LastActionUtc' AND data_type='datetime' AND datetime_precision=6 AND is_nullable='NO' AND generation_expression='')=1
        AND (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='ConnectedUsers' AND ((column_name='IPAdress' AND data_type='varchar' AND character_maximum_length=45) OR (column_name='UserAgent' AND data_type='varchar' AND character_maximum_length=255)) AND is_nullable='NO')=2
        AND (SELECT COUNT(*) FROM information_schema.statistics WHERE table_schema=DATABASE() AND table_name='ConnectedUsers' AND column_name='TokenHash' AND non_unique=0 AND sub_part IS NULL AND seq_in_index=1 AND index_name IN (SELECT index_name FROM information_schema.statistics WHERE table_schema=DATABASE() AND table_name='ConnectedUsers' GROUP BY index_name HAVING COUNT(*)=1))=1 THEN
        DO RELEASE_LOCK(lock_name);
        LEAVE main;
    END IF;
    IF (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='ConnectedUsers')<>7
        OR (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='ConnectedUsers' AND column_name IN ('ID','UserID','UserName','LoginKey','IPAdress','UserAgent','LastActionTime') AND is_nullable='NO' AND generation_expression='')<>7
        OR (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='ConnectedUsers' AND column_name IN ('UserName','LoginKey','IPAdress','UserAgent') AND data_type='varchar' AND character_maximum_length=255)<>4
        OR (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='ConnectedUsers' AND column_name='LastActionTime' AND data_type='datetime')<>1 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT='Unsupported or partial session schema; inspect before retry';
    END IF;
    SELECT MIN(index_name), COUNT(*) INTO old_index, old_index_count FROM (
        SELECT index_name FROM information_schema.statistics WHERE table_schema=DATABASE() AND table_name='ConnectedUsers' AND non_unique=0
        GROUP BY index_name HAVING GROUP_CONCAT(column_name ORDER BY seq_in_index)='UserName,LoginKey,IPAdress,UserAgent' AND COUNT(*)=4 AND SUM(sub_part IS NOT NULL)=0
    ) AS old_indexes;
    IF old_index IS NULL OR old_index_count<>1 THEN SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT='Exactly one full legacy session index required'; END IF;
    -- [Комментарий ИИ-агента]: Все известные препятствия ALTER проверяются до DELETE; пользовательские индексы и ограничения автоматически не удаляются.
    IF EXISTS (SELECT 1 FROM information_schema.statistics WHERE table_schema=DATABASE() AND table_name='ConnectedUsers' AND LOWER(index_name)=LOWER('IX_ConnectedUsers_TokenHash')) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT='Target session index name already exists; inspect without deleting data';
    END IF;
    IF (SELECT COUNT(*) FROM information_schema.statistics WHERE table_schema=DATABASE() AND table_name='ConnectedUsers' AND index_name='PRIMARY')<>1
        OR EXISTS (SELECT 1 FROM information_schema.statistics WHERE table_schema=DATABASE() AND table_name='ConnectedUsers' AND index_name<>old_index AND (column_name IN ('UserName','LoginKey') OR column_name IS NULL OR (column_name='IPAdress' AND sub_part>45))) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT='Unsupported user index or primary key; inspect without deleting data';
    END IF;
    IF (SELECT COUNT(*) FROM information_schema.key_column_usage WHERE table_schema=DATABASE() AND table_name='ConnectedUsers' AND referenced_table_name IS NOT NULL)<>1
        OR (SELECT COUNT(*) FROM information_schema.key_column_usage WHERE table_schema=DATABASE() AND table_name='ConnectedUsers' AND column_name='UserID' AND referenced_table_schema=DATABASE() AND referenced_table_name='Users' AND referenced_column_name='ID')<>1
        OR EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE table_schema=DATABASE() AND table_name='ConnectedUsers' AND constraint_type='CHECK')
        OR EXISTS (SELECT 1 FROM information_schema.triggers WHERE event_object_schema=DATABASE() AND event_object_table='ConnectedUsers') THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT='Unsupported session constraint or trigger; inspect without deleting data';
    END IF;
    DELETE FROM ConnectedUsers;
    SET @storecms_sessions_ddl=CONCAT('ALTER TABLE `ConnectedUsers` DROP INDEX `',REPLACE(old_index,'`','``'),
        '`, DROP COLUMN `UserName`, DROP COLUMN `LoginKey`, CHANGE COLUMN `LastActionTime` `LastActionUtc` datetime(6) NOT NULL,',
        ' ADD COLUMN `TokenHash` char(64) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,',
        ' MODIFY COLUMN `IPAdress` varchar(45) NOT NULL, MODIFY COLUMN `UserAgent` varchar(255) NOT NULL,',
        ' ADD UNIQUE INDEX `IX_ConnectedUsers_TokenHash` (`TokenHash`)');
    PREPARE storecms_sessions_stmt FROM @storecms_sessions_ddl;
    EXECUTE storecms_sessions_stmt;
    DEALLOCATE PREPARE storecms_sessions_stmt;
    SET @storecms_sessions_ddl=NULL;
    DO RELEASE_LOCK(lock_name);
END$$
CALL StoreCMS_UpgradeSessionsV2()$$
DROP PROCEDURE StoreCMS_UpgradeSessionsV2$$
DELIMITER ;
