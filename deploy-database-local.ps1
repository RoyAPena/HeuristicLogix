# ============================================================
# HeuristicLogix ERP - Local Database Deployment Script
# Target: LAPTOP-7MG6K7RV | Database: HeuristicLogix
# ============================================================

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "HeuristicLogix ERP - Database Deployment" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$ServerName = "LAPTOP-7MG6K7RV"
$DatabaseName = "HeuristicLogix"
$StartupProject = "HeuristicLogix.Api"
$MigrationName = "InitialERPDeployment"

Write-Host "?? Deployment Configuration:" -ForegroundColor Yellow
Write-Host "   Server: $ServerName" -ForegroundColor White
Write-Host "   Database: $DatabaseName" -ForegroundColor White
Write-Host "   Auth: Windows Authentication" -ForegroundColor White
Write-Host "   Startup Project: $StartupProject" -ForegroundColor White
Write-Host ""

# Step 1: Verify Connection String
Write-Host "Step 1: Verifying Connection String..." -ForegroundColor Green
$appsettingsPath = ".\HeuristicLogix.Api\appsettings.json"
if (Test-Path $appsettingsPath) {
    Write-Host "   ? appsettings.json found" -ForegroundColor White
    $content = Get-Content $appsettingsPath -Raw
    if ($content -match "LAPTOP-7MG6K7RV") {
        Write-Host "   ? Connection string configured for LAPTOP-7MG6K7RV" -ForegroundColor White
    } else {
        Write-Host "   ??  Connection string may need verification" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ? appsettings.json not found!" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 2: Check EF Core Tools
Write-Host "Step 2: Checking EF Core Tools..." -ForegroundColor Green
try {
    $efVersion = dotnet ef --version 2>&1
    Write-Host "   ? EF Core Tools installed: $efVersion" -ForegroundColor White
} catch {
    Write-Host "   ? EF Core Tools not found!" -ForegroundColor Red
    Write-Host "   Installing EF Core Tools..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
}
Write-Host ""

# Step 3: Verify SQL Server Connection
Write-Host "Step 3: Testing SQL Server Connection..." -ForegroundColor Green
try {
    $connectionString = "Server=$ServerName;Database=master;Trusted_Connection=True;TrustServerCertificate=True;"
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    Write-Host "   ? SQL Server connection successful" -ForegroundColor White
    
    # Check SQL Server version
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = "SELECT @@VERSION"
    $version = $cmd.ExecuteScalar()
    Write-Host "   ??  SQL Server Version: $($version.Split("`n")[0])" -ForegroundColor Cyan
    $connection.Close()
} catch {
    Write-Host "   ? Cannot connect to SQL Server: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Please ensure SQL Server is running and Windows Authentication is enabled." -ForegroundColor Yellow
    exit 1
}
Write-Host ""

# Step 4: Clean Previous Migrations (Optional)
Write-Host "Step 4: Checking for existing migrations..." -ForegroundColor Green
$migrationsFolder = ".\HeuristicLogix.Api\Migrations"
if (Test-Path $migrationsFolder) {
    $migrationCount = (Get-ChildItem $migrationsFolder -Filter "*.cs").Count
    if ($migrationCount -gt 0) {
        Write-Host "   ??  Found $migrationCount existing migration files" -ForegroundColor Yellow
        $response = Read-Host "   Do you want to delete existing migrations? (y/N)"
        if ($response -eq 'y' -or $response -eq 'Y') {
            Remove-Item $migrationsFolder -Recurse -Force
            Write-Host "   ? Existing migrations deleted" -ForegroundColor White
        } else {
            Write-Host "   ??  Keeping existing migrations" -ForegroundColor Cyan
        }
    }
} else {
    Write-Host "   ??  No existing migrations found" -ForegroundColor Cyan
}
Write-Host ""

# Step 5: Generate Migration
Write-Host "Step 5: Generating EF Core Migration..." -ForegroundColor Green
Write-Host "   Running: dotnet ef migrations add $MigrationName --project $StartupProject" -ForegroundColor Cyan

Push-Location $StartupProject
try {
    $migrationOutput = dotnet ef migrations add $MigrationName --verbose 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ? Migration '$MigrationName' created successfully" -ForegroundColor White
        Write-Host "   ??  Migration files created in: $StartupProject\Migrations" -ForegroundColor Cyan
    } else {
        Write-Host "   ? Migration generation failed!" -ForegroundColor Red
        Write-Host $migrationOutput -ForegroundColor Red
        Pop-Location
        exit 1
    }
} catch {
    Write-Host "   ? Error generating migration: $($_.Exception.Message)" -ForegroundColor Red
    Pop-Location
    exit 1
}
Pop-Location
Write-Host ""

# Step 6: Review Migration (Optional)
Write-Host "Step 6: Migration Ready for Review" -ForegroundColor Green
Write-Host "   Migration files are in: $StartupProject\Migrations" -ForegroundColor White
Write-Host "   Would you like to review the migration before applying? (y/N)" -ForegroundColor Yellow
$review = Read-Host
if ($review -eq 'y' -or $review -eq 'Y') {
    Write-Host "   Opening migration folder..." -ForegroundColor Cyan
    explorer.exe ".\$StartupProject\Migrations"
    Read-Host "   Press Enter when ready to continue"
}
Write-Host ""

# Step 7: Apply Migration
Write-Host "Step 7: Applying Migration to Database..." -ForegroundColor Green
Write-Host "   ??  This will create/update the database: $DatabaseName on $ServerName" -ForegroundColor Yellow
$confirm = Read-Host "   Proceed with database update? (y/N)"

if ($confirm -ne 'y' -and $confirm -ne 'Y') {
    Write-Host "   ??  Database update cancelled by user" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To apply manually later, run:" -ForegroundColor Yellow
    Write-Host "   cd $StartupProject" -ForegroundColor White
    Write-Host "   dotnet ef database update" -ForegroundColor White
    exit 0
}

Push-Location $StartupProject
try {
    Write-Host "   Running: dotnet ef database update --verbose" -ForegroundColor Cyan
    $updateOutput = dotnet ef database update --verbose 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ? Database updated successfully!" -ForegroundColor White
    } else {
        Write-Host "   ? Database update failed!" -ForegroundColor Red
        Write-Host $updateOutput -ForegroundColor Red
        Pop-Location
        exit 1
    }
} catch {
    Write-Host "   ? Error updating database: $($_.Exception.Message)" -ForegroundColor Red
    Pop-Location
    exit 1
}
Pop-Location
Write-Host ""

# Step 8: Enable RCSI
Write-Host "Step 8: Enabling Read Committed Snapshot Isolation (RCSI)..." -ForegroundColor Green
try {
    $connectionString = "Server=$ServerName;Database=master;Trusted_Connection=True;TrustServerCertificate=True;"
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    # Check if database exists
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = '$DatabaseName'"
    $dbExists = $cmd.ExecuteScalar()
    
    if ($dbExists -eq 1) {
        # Enable RCSI
        $cmd.CommandText = @"
ALTER DATABASE [$DatabaseName] SET READ_COMMITTED_SNAPSHOT ON;
"@
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "   ? RCSI enabled on database: $DatabaseName" -ForegroundColor White
        Write-Host "   ??  Multi-user concurrency optimized" -ForegroundColor Cyan
    } else {
        Write-Host "   ??  Database not found, RCSI not configured" -ForegroundColor Yellow
    }
    
    $connection.Close()
} catch {
    Write-Host "   ??  Warning: Could not enable RCSI: $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host "   You can enable it manually later with:" -ForegroundColor Cyan
    Write-Host "   ALTER DATABASE [$DatabaseName] SET READ_COMMITTED_SNAPSHOT ON;" -ForegroundColor White
}
Write-Host ""

# Step 9: Schema Verification
Write-Host "Step 9: Verifying Database Schema..." -ForegroundColor Green
try {
    $connectionString = "Server=$ServerName;Database=$DatabaseName;Trusted_Connection=True;TrustServerCertificate=True;"
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    # Check schemas
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = @"
SELECT 
    s.name AS SchemaName,
    COUNT(t.name) AS TableCount
FROM sys.schemas s
LEFT JOIN sys.tables t ON s.schema_id = t.schema_id
WHERE s.name IN ('Core', 'Inventory', 'Purchasing', 'Logistics')
GROUP BY s.name
ORDER BY s.name;
"@
    
    $reader = $cmd.ExecuteReader()
    $schemas = @{}
    while ($reader.Read()) {
        $schemaName = $reader["SchemaName"]
        $tableCount = $reader["TableCount"]
        $schemas[$schemaName] = $tableCount
    }
    $reader.Close()
    
    Write-Host "   ?? Schema Summary:" -ForegroundColor Cyan
    foreach ($schema in $schemas.Keys | Sort-Object) {
        Write-Host "      $schema : $($schemas[$schema]) tables" -ForegroundColor White
    }
    
    # Verify critical tables with ID types
    Write-Host ""
    Write-Host "   ?? Verifying Critical Tables & ID Types:" -ForegroundColor Cyan
    
    $cmd.CommandText = @"
SELECT 
    SCHEMA_NAME(t.schema_id) AS SchemaName,
    t.name AS TableName,
    c.name AS ColumnName,
    ty.name AS DataType,
    c.is_nullable AS IsNullable
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
WHERE t.name IN ('Items', 'Categories', 'Brands', 'UnitsOfMeasure', 
                 'Suppliers', 'TaxConfigurations', 'ItemUnitConversions')
    AND c.name LIKE '%Id'
    AND c.is_identity = 1 OR (c.name LIKE '%Id' AND c.column_id = 1)
ORDER BY SchemaName, TableName;
"@
    
    $reader = $cmd.ExecuteReader()
    $idTypes = @{}
    while ($reader.Read()) {
        $tableName = $reader["TableName"]
        $dataType = $reader["DataType"]
        $idTypes[$tableName] = $dataType
    }
    $reader.Close()
    
    # Verify expected ID types
    $expectedTypes = @{
        "Items" = "int"
        "Categories" = "int"
        "Brands" = "int"
        "UnitsOfMeasure" = "int"
        "Suppliers" = "uniqueidentifier"
        "TaxConfigurations" = "uniqueidentifier"
        "ItemUnitConversions" = "uniqueidentifier"
    }
    
    $allCorrect = $true
    foreach ($table in $expectedTypes.Keys | Sort-Object) {
        if ($idTypes.ContainsKey($table)) {
            $actual = $idTypes[$table]
            $expected = $expectedTypes[$table]
            if ($actual -eq $expected) {
                Write-Host "      ? $table : $actual (correct)" -ForegroundColor White
            } else {
                Write-Host "      ? $table : $actual (expected: $expected)" -ForegroundColor Red
                $allCorrect = $false
            }
        } else {
            Write-Host "      ??  $table : NOT FOUND" -ForegroundColor Yellow
            $allCorrect = $false
        }
    }
    
    $connection.Close()
    
    if ($allCorrect) {
        Write-Host ""
        Write-Host "   ? All ID types are correct!" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "   ??  Some ID types may need verification" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "   ??  Could not verify schema: $($_.Exception.Message)" -ForegroundColor Yellow
}
Write-Host ""

# Step 10: Summary
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "? Deployment Complete!" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Database Summary:" -ForegroundColor Yellow
Write-Host "  Server: $ServerName" -ForegroundColor White
Write-Host "  Database: $DatabaseName" -ForegroundColor White
Write-Host "  Status: Ready for use" -ForegroundColor Green
Write-Host ""
Write-Host "Hybrid ID Architecture:" -ForegroundColor Yellow
Write-Host "  • Inventory Entities: int IDs (Category, Brand, Item, UnitOfMeasure)" -ForegroundColor White
Write-Host "  • Core/Purchasing: Guid IDs (TaxConfiguration, Supplier, Staging)" -ForegroundColor White
Write-Host "  • RCSI: Enabled for multi-user concurrency" -ForegroundColor White
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Verify the database in SQL Server Management Studio" -ForegroundColor White
Write-Host "  2. Seed initial data (tax configurations, units of measure, etc.)" -ForegroundColor White
Write-Host "  3. Test the API connection to the database" -ForegroundColor White
Write-Host ""
Write-Host "Connection String:" -ForegroundColor Yellow
Write-Host "  Server=$ServerName;Database=$DatabaseName;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;" -ForegroundColor Cyan
Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan

# Optional: Open SQL Server Management Studio
$openSSMS = Read-Host "Would you like to open the database in SSMS? (y/N)"
if ($openSSMS -eq 'y' -or $openSSMS -eq 'Y') {
    try {
        Start-Process "ssms.exe" -ArgumentList "-S $ServerName -d $DatabaseName"
        Write-Host "   ??  Opening SSMS..." -ForegroundColor Cyan
    } catch {
        Write-Host "   ??  Could not open SSMS. Please open manually." -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "?? HeuristicLogix ERP Database is ready!" -ForegroundColor Green
