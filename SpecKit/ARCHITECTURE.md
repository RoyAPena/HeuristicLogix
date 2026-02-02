# Architecture Specification - HeuristicLogix

## 1. Technical Foundation
- **Solution Name:** HeuristicLogix
- **Framework:** .NET 10 (C# 14)
- **Frontend:** Blazor WebAssembly Standalone
- **UI Library:** MudBlazor (Open Source)
- **Architecture Pattern:** Vertical Slice Architecture
- **External APIs:** Google Maps Platform (Routes & Geocoding)
- **Database:** SQL Server (On-Premise) / Azure SQL with Entity Framework Core.

## 2. Naming & Semantic Standards (Strict)
To ensure long-term maintainability and AI data readiness, we prioritize clarity over brevity.

### 2.1 Identifiers and Columns
- **No Abbreviations:** Use full, descriptive words. 
    - *Bad:* `RNC`, `Qty`, `UoM`, `Desc`, `TaxId`.
    - *Good:* `NationalTaxIdentificationNumber`, `CurrentStockQuantity`, `UnitOfMeasure`, `ItemDescription`, `TaxConfigurationId`.
- **Primary Keys:** Table Name + `Id`. Example: `SupplierId`, `PurchaseInvoiceId`.
- **Foreign Keys:** Must mirror the Target Table Primary Key name exactly.

### 2.2 Database Schemas
Logic must be partitioned into SQL Schemas:
- **[Core]:** Shared configurations (Taxes, Global settings).
- **[Inventory]:** Products, Brands, Categories, Unit Conversions.
- **[Purchasing]:** Suppliers, Purchase Invoices, Staging tables.
- **[Logistics]:** Trucks, Routes, Geocoding (Phase 2).

## 3. Development & Data Standards
- **Modern C#:** Use Primary Constructors, Collection Expressions, and Required Members.
- **Precision:** Use `DECIMAL(18,4)` for monetary amounts and `DECIMAL(18,2)` for quantities.
- **Enum Handling:** - **Strict Requirement:** All Enums must be persisted as **Strings**, never as Integers.
    - **Implementation:** Use `JsonStringEnumConverter` for JSON and EF Core Value Converters for DB.
- **Geospatial Constraint:** No 'Conduce' can be persisted without validated Latitude/Longitude coordinates.

## 4. AI & Performance Patterns
- **AI Data Readiness:** Schema must include `AIPredictedTime`, `ExpertDecisionTime`, and `ActualServiceTime` for future ML training.
- **Staging Area Pattern:** For heavy data ingestion (e.g., Mass Invoice Scanning), use Staging tables to prevent DB locking before final atomic commit.
- **Concurrency:** Designed for **Read Committed Snapshot Isolation (RCSI)** to allow non-blocking reads.