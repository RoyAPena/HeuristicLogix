# SpecKit: Master Architecture & Semantic Standards

## 1. Mission: AI-Data Readiness & Semantic Integrity
> "Prioritize absolute clarity over brevity. The codebase must be self-documenting to facilitate future Machine Learning training."

### 1.1 Zero Abbreviation Policy (Strict)
All properties, entities, and database columns must use full semantic names.
- **Forbidden:** `RNC`, `Qty`, `UoM`, `Desc`, `Cat`, `Id`, `Price`.
- **Mandatory:** - `NationalTaxIdentificationNumber` (instead of RNC).
    - `CurrentStockQuantity` (instead of Qty).
    - `UnitOfMeasure` (instead of UoM).
    - `ItemDescription` (instead of Desc).
    - `ProductCategory` (instead of Cat).
    - **Primary Keys:** Must be `[EntityName]Id` (e.g., `BrandId`, `SupplierId`).

### 1.2 Hybrid Identifier Strategy
- **Inventory Module:** Uses `INT` identifiers for performance and legacy compatibility (e.g., `BrandId`, `CategoryId`).
- **Purchasing/Core Modules:** Uses `GUID` identifiers for transactional integrity (e.g., `SupplierId`, `TaxConfigurationId`).

## 2. Execution: Vertical Slice + MediatR
- **Pattern:** Every action is a "Slice". Traditional shared services are prohibited.
- **Components:** Each feature folder must contain:
    1. **Command/Query:** `public record` intent.
    2. **Validator:** `FluentValidation` rules.
    3. **Handler:** Implementation of `IRequestHandler<T, Result<R>>`.
- **Result Pattern:** All handlers MUST return a `Result<T>` object.

## 3. Physical Isolation (Modular Monolith)
- **Projects:** Each module resides in its own `.csproj` (e.g., `HeuristicLogix.Modules.Inventory`).
- **Communication:** Modules **MUST NOT** reference each other directly. Use `HeuristicLogix.Shared` for cross-module contracts.
- **Persistence:** Use `IApplicationDbContext` for abstraction. Each module owns its `EntityTypeConfiguration<T>`.

## 4. Database Standards
- **Schemas:** Data must be partitioned into: `[Core]`, `[Inventory]`, `[Purchasing]`, `[Logistics]`.
- **Enums:** Must ALWAYS be persisted as **Strings** in the database.
- **Precision:** `DECIMAL(18,4)` for monetary amounts and `DECIMAL(18,2)` for quantities.