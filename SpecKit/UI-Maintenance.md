# SpecKit: Base Maintenance Infrastructure

## 1. Goal
Standardize CRUD operations for master data while ensuring high performance, scannability, and type safety for our hybrid ID system (int/Guid).

## 2. Component Architecture (The Base-Implementation Pattern)
- **Base Component (`MaintenanceBase<TEntity>`)**: An abstract MudBlazor component handling:
    - Table states (Loading, Search, Pagination).
    - CRUD logic (OpenDialog, Save, Delete).
    - Snackbar notifications and Error Handling.
- **Specific Implementation**: Concrete pages (e.g., `BrandPage.razor`) that inherit from the base.
- **Constraint**: NO Reflection. Specific pages must explicitly define Table Columns and Dialog Fields via RenderFragments.

## 3. Data & Validation
- **Services**: Use `IBaseMaintenanceService<TEntity>` for standard data access.
- **Validation**: Strict use of **FluentValidation** (Clean Code: no validation logic in Razor).
- **IDs**: Must support both `int` (Inventory) and `Guid` (Core/Purchasing).

## 4. UI Standards (MudBlazor)
- **Density**: `Dense="true"` for all tables.
- **Feedback**: `MudProgressLinear` for loading; `MudDialog` for all data entry.
- **Icons**: Edit (`Icons.Material.Filled.Edit`), Delete (`Icons.Material.Filled.Delete`).