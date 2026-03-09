-- =============================================================
-- Migration: Material Standards Refactor
-- Creates Materials and JobMaterialLinks tables, seeds data,
-- and drops the obsolete MaterialInferred column.
-- =============================================================

-- 1. Create Materials table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Materials')
BEGIN
    CREATE TABLE [Materials] (
        [Id] INT NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(200) NOT NULL,
        [Category] NVARCHAR(100) NOT NULL,
        [AmsDesignation] NVARCHAR(50) NULL,
        [UnsDesignation] NVARCHAR(50) NULL,
        [IsoDesignation] NVARCHAR(100) NULL,
        [Form] NVARCHAR(50) NULL,
        [TemperCondition] NVARCHAR(50) NULL
    );
    PRINT 'Created Materials table.';
END
ELSE
    PRINT 'Materials table already exists.';
GO

-- 2. Create JobMaterialLinks join table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'JobMaterialLinks')
BEGIN
    CREATE TABLE [JobMaterialLinks] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [JobId] VARCHAR(100) NOT NULL,
        [MaterialId] INT NOT NULL,
        CONSTRAINT [FK_JobMaterialLinks_JobLeads] FOREIGN KEY ([JobId]) REFERENCES [JobLeads]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_JobMaterialLinks_Materials] FOREIGN KEY ([MaterialId]) REFERENCES [Materials]([Id]) ON DELETE CASCADE,
        CONSTRAINT [UQ_JobMaterialLinks_Job_Material] UNIQUE ([JobId], [MaterialId])
    );
    PRINT 'Created JobMaterialLinks table.';
END
ELSE
    PRINT 'JobMaterialLinks table already exists.';
GO

-- 3. Seed Materials data
MERGE INTO [Materials] AS target
USING (VALUES
    (1, 'Aluminum 2024-T3', 'Aluminum', 'AMS 4037', 'A92024', 'AlCu4Mg1', 'Sheet', 'T3'),
    (2, 'Aluminum 2024-T351', 'Aluminum', 'AMS 4035', 'A92024', 'AlCu4Mg1', 'Plate', 'T351'),
    (3, 'Aluminum 6061-T6', 'Aluminum', 'AMS 4027', 'A96061', 'AlMg1SiCu', 'Bar', 'T6'),
    (4, 'Aluminum 6061-T651', 'Aluminum', 'AMS 4027', 'A96061', 'AlMg1SiCu', 'Plate', 'T651'),
    (5, 'Aluminum 7075-T6', 'Aluminum', 'AMS 4045', 'A97075', 'AlZn5.5MgCu', 'Bar', 'T6'),
    (6, 'Aluminum 7075-T651', 'Aluminum', 'AMS 4078', 'A97075', 'AlZn5.5MgCu', 'Plate', 'T651'),
    (7, 'Aluminum 7050-T7451', 'Aluminum', 'AMS 4050', 'A97050', 'AlZn6CuMgZr', 'Plate', 'T7451'),
    (8, 'Ti-6Al-4V (Grade 5)', 'Titanium', 'AMS 4911', 'R56400', 'TiAl6V4', 'Bar', 'Annealed'),
    (9, 'Ti-6Al-4V ELI (Grade 23)', 'Titanium', 'AMS 4930', 'R56401', 'TiAl6V4 ELI', 'Bar', 'Annealed'),
    (10, 'CP Titanium Grade 2', 'Titanium', 'AMS 4902', 'R50400', 'Ti-Gr2', 'Sheet', 'Annealed'),
    (11, 'Ti-6Al-4V STA', 'Titanium', 'AMS 4928', 'R56400', 'TiAl6V4', 'Forging', 'STA'),
    (12, 'Inconel 718', 'Nickel', 'AMS 5663', 'N07718', 'NiCr19Fe19Nb5Mo3', 'Bar', 'Aged'),
    (13, 'Inconel 625', 'Nickel', 'AMS 5666', 'N06625', 'NiCr22Mo9Nb', 'Sheet', 'Annealed'),
    (14, 'Waspaloy', 'Nickel', 'AMS 5544', 'N07001', 'NiCr20Co13Mo4Ti3Al', 'Forging', 'Aged'),
    (15, 'Hastelloy X', 'Nickel', 'AMS 5536', 'N06002', 'NiCr22Fe18Mo', 'Sheet', 'Solution Treated'),
    (16, 'Monel K-500', 'Nickel', 'AMS 4676', 'N05500', 'NiCu30Al', 'Bar', 'Age Hardened'),
    (17, '304 Stainless Steel', 'Steel', 'AMS 5513', 'S30400', 'X5CrNi18-10', 'Sheet', 'Annealed'),
    (18, '316L Stainless Steel', 'Steel', 'AMS 5507', 'S31603', 'X2CrNiMo17-12-2', 'Bar', 'Annealed'),
    (19, '17-4 PH Stainless', 'Steel', 'AMS 5643', 'S17400', 'X5CrNiCuNb16-4', 'Bar', 'H900'),
    (20, '15-5 PH Stainless', 'Steel', 'AMS 5659', 'S15500', 'X5CrNiCuNb15-5', 'Bar', 'H1025'),
    (21, 'A286 Iron-Based Super', 'Steel', 'AMS 5731', 'S66286', 'X5NiCrTi26-15', 'Bar', 'Aged'),
    (22, '4340 Alloy Steel', 'Steel', 'AMS 6414', 'G43400', '34CrNiMo6', 'Bar', 'Quenched & Tempered'),
    (23, '4130 Alloy Steel', 'Steel', 'AMS 6370', 'G41300', '25CrMo4', 'Tube', 'Normalized'),
    (24, '300M Ultra-High Strength', 'Steel', 'AMS 6417', 'K44220', '300M', 'Bar', 'Quenched & Tempered'),
    (25, 'Copper C110 (ETP)', 'Copper', NULL, 'C11000', 'Cu-ETP', 'Bar', 'Half Hard'),
    (26, 'Beryllium Copper C172', 'Copper', 'AMS 4533', 'C17200', 'CuBe2', 'Bar', 'AT'),
    (27, 'Magnesium AZ31B', 'Magnesium', 'AMS 4375', 'M11311', 'MgAl3Zn1', 'Sheet', 'H24')
) AS source (Id, [Name], Category, AmsDesignation, UnsDesignation, IsoDesignation, Form, TemperCondition)
ON target.Id = source.Id
WHEN NOT MATCHED THEN
    INSERT (Id, [Name], Category, AmsDesignation, UnsDesignation, IsoDesignation, Form, TemperCondition)
    VALUES (source.Id, source.[Name], source.Category, source.AmsDesignation, source.UnsDesignation, source.IsoDesignation, source.Form, source.TemperCondition);
PRINT 'Seeded Materials data.';
GO

-- 4. Drop obsolete MaterialInferred column
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'JobLeads' AND COLUMN_NAME = 'MaterialInferred')
BEGIN
    ALTER TABLE [JobLeads] DROP COLUMN [MaterialInferred];
    PRINT 'Dropped MaterialInferred column from JobLeads.';
END
ELSE
    PRINT 'MaterialInferred column does not exist (already removed).';
GO

PRINT 'Migration complete.';
GO
