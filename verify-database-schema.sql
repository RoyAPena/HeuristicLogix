-- ============================================================
-- HeuristicLogix ERP - Database Verification Script
-- Target: LAPTOP-7MG6K7RV | Database: HeuristicLogix
-- Purpose: Verify schema structure, ID types, and constraints
-- ============================================================

USE [HeuristicLogix];
GO

PRINT '============================================================';
PRINT 'HeuristicLogix ERP - Database Verification';
PRINT '============================================================';
PRINT '';

-- Step 1: Verify Database Settings
PRINT 'Step 1: Database Configuration';
PRINT '------------------------------------------------------------';

SELECT 
    'Database Name' AS Setting,
    DB_NAME() AS Value
UNION ALL
SELECT 
    'RCSI Enabled',
    CASE WHEN is_read_committed_snapshot_on = 1 THEN 'YES' ELSE 'NO' END
FROM sys.databases
WHERE name = DB_NAME();

PRINT '';

-- Step 2: Schema Summary
PRINT 'Step 2: Schema Summary';
PRINT '------------------------------------------------------------';

SELECT 
    s.name AS SchemaName,
    COUNT(t.name) AS TableCount
FROM sys.schemas s
LEFT JOIN sys.tables t ON s.schema_id = t.schema_id
WHERE s.name IN ('Core', 'Inventory', 'Purchasing', 'Logistics')
GROUP BY s.name
ORDER BY s.name;

PRINT '';

-- Step 3: Detailed Table Listing by Schema
PRINT 'Step 3: Tables by Schema';
PRINT '------------------------------------------------------------';

SELECT 
    SCHEMA_NAME(t.schema_id) AS SchemaName,
    t.name AS TableName,
    (SELECT COUNT(*) FROM sys.columns WHERE object_id = t.object_id) AS ColumnCount,
    (SELECT COUNT(*) FROM sys.indexes WHERE object_id = t.object_id AND is_primary_key = 1) AS HasPK
FROM sys.tables t
WHERE SCHEMA_NAME(t.schema_id) IN ('Core', 'Inventory', 'Purchasing', 'Logistics')
ORDER BY SchemaName, TableName;

PRINT '';

-- Step 4: ID Type Verification (Hybrid Architecture)
PRINT 'Step 4: Primary Key ID Types (Hybrid Architecture)';
PRINT '------------------------------------------------------------';
PRINT 'Expected: Inventory = int | Core/Purchasing = Guid';
PRINT '';

SELECT 
    SCHEMA_NAME(t.schema_id) AS SchemaName,
    t.name AS TableName,
    c.name AS PKColumnName,
    TYPE_NAME(c.user_type_id) AS DataType,
    CASE 
        -- Inventory tables should be int
        WHEN t.name IN ('Categories', 'Brands', 'Items', 'UnitsOfMeasure') AND TYPE_NAME(c.user_type_id) = 'int' THEN '? Correct'
        -- Core/Purchasing tables should be uniqueidentifier
        WHEN t.name IN ('TaxConfigurations', 'Suppliers', 'StagingPurchaseInvoices', 'StagingPurchaseInvoiceDetails') 
            AND TYPE_NAME(c.user_type_id) = 'uniqueidentifier' THEN '? Correct'
        -- Bridge entity (Guid PK)
        WHEN t.name = 'ItemUnitConversions' AND TYPE_NAME(c.user_type_id) = 'uniqueidentifier' THEN '? Correct'
        -- Composite PK (ItemSuppliers)
        WHEN t.name = 'ItemSuppliers' THEN '? Composite'
        ELSE '? MISMATCH'
    END AS Status
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.is_primary_key = 1
    AND SCHEMA_NAME(t.schema_id) IN ('Core', 'Inventory', 'Purchasing')
    AND t.name NOT LIKE '%History%'
ORDER BY SchemaName, TableName;

PRINT '';

-- Step 5: Foreign Key Verification
PRINT 'Step 5: Foreign Key Constraints';
PRINT '------------------------------------------------------------';

SELECT 
    SCHEMA_NAME(fk.schema_id) AS SchemaName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    fk.name AS ForeignKeyName,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS FKColumn,
    SCHEMA_NAME(pk_tab.schema_id) AS ReferencedSchema,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ReferencedColumn,
    CASE fk.delete_referential_action
        WHEN 0 THEN 'NO ACTION (Restrict)'
        WHEN 1 THEN 'CASCADE'
        WHEN 2 THEN 'SET NULL'
        WHEN 3 THEN 'SET DEFAULT'
    END AS DeleteBehavior
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.tables pk_tab ON fk.referenced_object_id = pk_tab.object_id
WHERE SCHEMA_NAME(fk.schema_id) IN ('Core', 'Inventory', 'Purchasing')
ORDER BY SchemaName, TableName, ForeignKeyName;

PRINT '';

-- Step 6: Unique Constraints Verification
PRINT 'Step 6: Unique Constraints';
PRINT '------------------------------------------------------------';

SELECT 
    SCHEMA_NAME(t.schema_id) AS SchemaName,
    t.name AS TableName,
    i.name AS IndexName,
    COL_NAME(ic.object_id, ic.column_id) AS ColumnName,
    CASE 
        WHEN t.name = 'Items' AND COL_NAME(ic.object_id, ic.column_id) = 'StockKeepingUnitCode' THEN '? Expected'
        WHEN t.name = 'Suppliers' AND COL_NAME(ic.object_id, ic.column_id) = 'NationalTaxIdentificationNumber' THEN '? Expected'
        WHEN t.name = 'UnitsOfMeasure' AND COL_NAME(ic.object_id, ic.column_id) = 'UnitOfMeasureSymbol' THEN '? Expected'
        ELSE '? Check'
    END AS Status
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE i.is_unique = 1
    AND i.is_primary_key = 0
    AND SCHEMA_NAME(t.schema_id) IN ('Core', 'Inventory', 'Purchasing')
ORDER BY SchemaName, TableName, IndexName;

PRINT '';

-- Step 7: Decimal Precision Verification
PRINT 'Step 7: Decimal Precision (Prices & Quantities)';
PRINT '------------------------------------------------------------';
PRINT 'Expected: Prices = (18,4) | Quantities = (18,2)';
PRINT '';

SELECT 
    SCHEMA_NAME(t.schema_id) AS SchemaName,
    t.name AS TableName,
    c.name AS ColumnName,
    TYPE_NAME(c.user_type_id) AS DataType,
    c.precision AS Precision,
    c.scale AS Scale,
    CASE 
        -- Price columns should be (18,4)
        WHEN c.name LIKE '%Price%' AND c.precision = 18 AND c.scale = 4 THEN '? Correct'
        WHEN c.name LIKE '%Cost%' AND c.precision = 18 AND c.scale = 4 THEN '? Correct'
        WHEN c.name LIKE '%Amount%' AND c.precision = 18 AND c.scale = 4 THEN '? Correct'
        -- Quantity columns should be (18,2)
        WHEN c.name LIKE '%Quantity%' AND c.precision = 18 AND c.scale = 2 THEN '? Correct'
        WHEN c.name LIKE '%Stock%' AND c.precision = 18 AND c.scale = 2 THEN '? Correct'
        -- Conversion factors should be (18,4)
        WHEN c.name = 'ConversionFactorQuantity' AND c.precision = 18 AND c.scale = 4 THEN '? Correct'
        -- Tax rates should be (5,2)
        WHEN c.name = 'TaxPercentageRate' AND c.precision = 5 AND c.scale = 2 THEN '? Correct'
        ELSE '? Check'
    END AS Status
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
WHERE TYPE_NAME(c.user_type_id) = 'decimal'
    AND SCHEMA_NAME(t.schema_id) IN ('Core', 'Inventory', 'Purchasing')
ORDER BY SchemaName, TableName, ColumnName;

PRINT '';

-- Step 8: Item Table Detail (Central Entity)
PRINT 'Step 8: Item Table Structure (Central Entity)';
PRINT '------------------------------------------------------------';

SELECT 
    c.column_id AS Ordinal,
    c.name AS ColumnName,
    TYPE_NAME(c.user_type_id) AS DataType,
    CASE WHEN c.is_nullable = 0 THEN 'NOT NULL' ELSE 'NULL' END AS Nullable,
    CASE 
        WHEN TYPE_NAME(c.user_type_id) = 'decimal' 
        THEN CONCAT('(', c.precision, ',', c.scale, ')')
        WHEN TYPE_NAME(c.user_type_id) IN ('nvarchar', 'varchar', 'char', 'nchar') 
        THEN CONCAT('(', CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length AS VARCHAR) END, ')')
        ELSE ''
    END AS Precision,
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM sys.index_columns ic
            INNER JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
            WHERE ic.object_id = c.object_id AND ic.column_id = c.column_id AND i.is_primary_key = 1
        ) THEN 'PK'
        WHEN c.name LIKE '%Id' AND c.name <> 'ItemId' THEN 'FK'
        ELSE ''
    END AS KeyType
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
WHERE t.name = 'Items'
    AND SCHEMA_NAME(t.schema_id) = 'Inventory'
ORDER BY c.column_id;

PRINT '';

-- Step 9: Row Counts (Initial State)
PRINT 'Step 9: Table Row Counts';
PRINT '------------------------------------------------------------';

SELECT 
    SCHEMA_NAME(t.schema_id) AS SchemaName,
    t.name AS TableName,
    p.rows AS RowCount
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.object_id
WHERE p.index_id IN (0, 1)
    AND SCHEMA_NAME(t.schema_id) IN ('Core', 'Inventory', 'Purchasing', 'Logistics')
ORDER BY SchemaName, TableName;

PRINT '';

-- Step 10: Navigation Properties Verification (FK Count)
PRINT 'Step 10: Entity Relationship Summary';
PRINT '------------------------------------------------------------';

SELECT 
    SCHEMA_NAME(t.schema_id) AS SchemaName,
    t.name AS TableName,
    (SELECT COUNT(*) 
     FROM sys.foreign_keys fk 
     WHERE fk.parent_object_id = t.object_id) AS OutgoingFKs,
    (SELECT COUNT(*) 
     FROM sys.foreign_keys fk 
     WHERE fk.referenced_object_id = t.object_id) AS IncomingFKs,
    (SELECT COUNT(*) 
     FROM sys.foreign_keys fk 
     WHERE fk.parent_object_id = t.object_id 
     OR fk.referenced_object_id = t.object_id) AS TotalRelationships
FROM sys.tables t
WHERE SCHEMA_NAME(t.schema_id) IN ('Core', 'Inventory', 'Purchasing')
ORDER BY TotalRelationships DESC, SchemaName, TableName;

PRINT '';
PRINT '============================================================';
PRINT 'Verification Complete!';
PRINT '============================================================';
PRINT '';
PRINT 'Summary Checks:';
PRINT '  ? Check Schema Summary (Core, Inventory, Purchasing)';
PRINT '  ? Check ID Types (int vs Guid hybrid architecture)';
PRINT '  ? Check Foreign Keys (Restrict vs Cascade)';
PRINT '  ? Check Unique Constraints (SKU, RNC, UOM Symbol)';
PRINT '  ? Check Decimal Precision (18,4 for prices, 18,2 for quantities)';
PRINT '';

-- Optional: Enable RCSI if not already enabled
DECLARE @RCSI bit;
SELECT @RCSI = is_read_committed_snapshot_on 
FROM sys.databases 
WHERE name = DB_NAME();

IF @RCSI = 0
BEGIN
    PRINT '??  RCSI is NOT enabled. Run this command to enable:';
    PRINT '   ALTER DATABASE [HeuristicLogix] SET READ_COMMITTED_SNAPSHOT ON;';
    PRINT '';
END
ELSE
BEGIN
    PRINT '? RCSI is enabled (optimal for multi-user concurrency)';
    PRINT '';
END

GO
