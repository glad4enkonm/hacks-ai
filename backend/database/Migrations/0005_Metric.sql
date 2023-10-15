USE ${MYSQL_DATABASE};

CREATE TABLE `Metric` (
    `MetricId` bigint unsigned auto_increment NOT NULL PRIMARY KEY,
    `CalculationDate` datetime NOT NULL,
    `Loaction` varchar(5000) NOT NULL,
    `PositiveOverNegative` decimal NULL,
    `EducationalDistance` decimal NULL,
    `CityDistrictMetric` decimal NULL,
    `ReserectionalDistance` decimal NULL
);


INSERT INTO Migration (Description) VALUES('${THIS_FILE_NAME}');