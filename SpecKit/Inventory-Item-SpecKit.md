# SpecKit: Inventory Item Module

## 1. Data Structure (SQL Server - Source of Truth)
- **Entity**: `Item`
- **Fields**:
    - `ItemId`: int (PK)
    - `SKU`: nvarchar(50) (Unique Index)
    - `Name`: nvarchar(500)
    - `Description`: nvarchar(max)
    - `CategoryId`: int (FK)
    - `BrandId`: int (FK)
    - `UnitOfMeasureId`: int (FK)
    - `CostPrice`: decimal(18,4)
    - `SalePrice`: decimal(18,4)
    - `MinimumStock`: decimal(18,4)
    - `CurrentStock`: decimal(18,4) (Updated via Transactions)
    - `IsActive`: bool

## 2. Search Strategy (Elasticsearch)
- **Search Model**: `ItemSearchModel` (Flattened Document)
- **Indexed Fields**: `Name` (Fuzzy), `SKU` (Exact), `BrandName`, `CategoryName`.
- **Sync Pattern**: "Update-on-Write". The `ItemService` will trigger an asynchronous index update to Elasticsearch after a successful SQL commit.

## 3. UI Requirements
- **Creation Dialog**: Must use `MudAutocomplete` for Category, Brand, and UnitOfMeasure to handle large catalogs without loading thousands of items in a standard Select.
- **Validation**: 
    - `SalePrice` must be > `CostPrice`.
    - `SKU` must be unique.