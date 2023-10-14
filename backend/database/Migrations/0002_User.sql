USE ${MYSQL_DATABASE};

CREATE TABLE `User`
(
    `UserId`       bigint unsigned auto_increment NOT NULL PRIMARY KEY,
    `Name`         varchar(500)                   NOT NULL,
    `Description`  varchar(1000)                  NULL     DEFAULT NULL,
    `Login`        varchar(100)                   NOT NULL UNIQUE,
    `PasswordHash` varchar(500)                   NOT NULL,
    `IsDeleted`    bool                           NOT NULL DEFAULT 0
);

INSERT INTO `User` (`Name`, `Description`, `Login`, `PasswordHash`)
VALUES ('Administrator', 'Управляет настройками',
        'superuser', '${SUPERUSER_PWD_HASH}');

CREATE TABLE `UserHistory`
(
    CONSTRAINT FK_UserHistory_EntityId_UserId FOREIGN KEY (EntityId) REFERENCES `User` (UserId),
    CONSTRAINT FK_UserHistory_ChangedBy_UserId FOREIGN KEY (ChangedBy) REFERENCES `User` (UserId),
    `UserHistoryId` bigint unsigned auto_increment NOT NULL PRIMARY KEY,
    `Changed`       datetime                       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `Difference`    varchar(1000)                  NOT NULL,
    `EntityId`      bigint unsigned                NOT NULL,
    `ChangedBy`     bigint unsigned                NOT NULL
);

CREATE TABLE RefreshToken
(
    RefreshTokenId  bigint unsigned auto_increment NOT NULL PRIMARY KEY,
    Token           varchar(1000)                  NOT NULL,
    Expires         DATETIME                       NOT NULL,
    Created         DATETIME                       NOT NULL,
    CreatedByIp     varchar(300)                   NOT NULL,
    Revoked         DATETIME                       NULL,
    RevokedByIp     varchar(300)                   NULL,
    ReplacedByToken varchar(1000)                  NULL,
    ReasonRevoked   varchar(500)                   NULL
);

CREATE TABLE UserToRefreshToken
(
    UserToRefreshTokenId bigint unsigned auto_increment NOT NULL PRIMARY KEY,
    UserId               bigint unsigned                NOT NULL,
    RefreshTokenId       bigint unsigned                NOT NULL,
    CONSTRAINT FK_UserId FOREIGN KEY (UserId) REFERENCES User (UserId),
    CONSTRAINT FK_RefreshToken FOREIGN KEY (RefreshTokenId) REFERENCES RefreshToken (RefreshTokenId)
        ON DELETE CASCADE
        ON UPDATE RESTRICT
);

INSERT INTO Migration (Description) VALUES ('${THIS_FILE_NAME}');