ALTER DATABASE ${MYSQL_DATABASE} COLLATE = 'utf8mb4_unicode_ci';
ALTER DATABASE ${MYSQL_DATABASE} CHARACTER SET = 'utf8mb4';

USE ${MYSQL_DATABASE};


CREATE TABLE `Code`
(
    `CodeId`      bigint unsigned auto_increment NOT NULL PRIMARY KEY,
    `Value`       varchar(100)                   NOT NULL UNIQUE,
    `Name`        varchar(1000)                  NULL,
    `Email`       varchar(500)                   NULL,
    `Telegram`    varchar(500)                   NULL,
    `Whatsapp`    varchar(500)                   NULL,
    `Balance`     decimal(10, 2)                 NOT NULL,
    `Completed`   datetime                       NULL,
    `Created`     datetime                       NOT NULL,
    `Description` varchar(1000)                  NULL     DEFAULT NULL,
    `IsDeleted`   bool                           NOT NULL DEFAULT 0
);

CREATE TABLE Migration
(
    MigrationId bigint unsigned auto_increment NOT NULL PRIMARY KEY,
    Description varchar(500)                   NULL
);

# Создаём отдельного пользователя для backend
CREATE USER '${BACKEND_DATABASE_USER}'@'${BACKEND_USER_HOST_FOR_DATABASE}' IDENTIFIED BY '${BACKEND_DATABASE_PASSWORD}';
GRANT ALL privileges on ${MYSQL_DATABASE}.* to '${BACKEND_DATABASE_USER}'@'${BACKEND_USER_HOST_FOR_DATABASE}';
FLUSH PRIVILEGES;

INSERT INTO Migration (Description) VALUES ('${THIS_FILE_NAME}');