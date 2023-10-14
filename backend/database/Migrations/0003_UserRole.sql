USE ${MYSQL_DATABASE};

CREATE TABLE `UserRole` (
    `UserRoleId` bigint unsigned auto_increment NOT NULL PRIMARY KEY,
    `Name` varchar(500) NOT NULL UNIQUE,
    `Description` varchar(1000) NULL DEFAULT NULL
);

INSERT INTO `UserRole` (`Name`, `Description`) VALUES('Administrator', 'Управляет сотрудниками и кодами');
INSERT INTO `UserRole` (`Name`, `Description`) VALUES('Operator', 'Сотрудник, создаёт коды');


INSERT INTO Migration (Description) VALUES('${THIS_FILE_NAME}');