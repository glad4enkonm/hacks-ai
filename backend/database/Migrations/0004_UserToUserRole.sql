USE ${MYSQL_DATABASE};

CREATE TABLE `UserToUserRole` (
    CONSTRAINT FK_UserToUserRole_UserId_UserId FOREIGN KEY (UserId) REFERENCES `User` (UserId),
    CONSTRAINT FK_UserToUserRole_UserRoleId_UserRoleId FOREIGN KEY (UserRoleId) REFERENCES `UserRole` (UserRoleId),
    `UserToUserRoleId` bigint unsigned auto_increment NOT NULL PRIMARY KEY,
    `UserId` bigint unsigned NOT NULL,
    `UserRoleId` bigint unsigned NOT NULL
);

INSERT INTO UserToUserRole (UserId, UserRoleId) VALUES ( 1, 1); # добавляем роль администратора первому пользователю

INSERT INTO Migration (Description) VALUES('${THIS_FILE_NAME}');