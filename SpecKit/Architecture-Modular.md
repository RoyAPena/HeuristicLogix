# SpecKit: Modular Monolith Physical Isolation

## 1. Physical Structure
- **Strict Separation:** Every module must reside in its own `.csproj` (Class Library).
- **Project Naming:** `HeuristicLogix.Modules.[ModuleName]`.
- **References:** Modules MUST NOT reference each other. They only reference `HeuristicLogix.Shared`.
- **Host:** `HeuristicLogix.Server` (API) is the only project that references all modules to bootstrap them.

## 2. Module Communication (Future-Proofing)
- **Contracts:** Interfaces for cross-module communication must live in `HeuristicLogix.Shared`.
- **Implementation:** The module owns the implementation.
- **Data:** Each module is responsible for its own Data Access configurations (Fluent API) via `EntityTypeConfiguration<T>`.

## 3. UI Features (Blazor)
- **Feature Folders:** The Client project (`HeuristicLogix.ERP`) will organize UI by feature: `/Features/[ModuleName]`.
- **Decoupling:** Use the `MaintenanceBase<T>` to ensure UI consistency across modules without code duplication.