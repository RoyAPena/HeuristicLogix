# SpecKit: UI View & Maintenance Standards

## 1. Pattern: Declarative Implementation
- All maintenance pages MUST inherit or encapsulate `MaintenanceBase<TEntity>`.
- **Logic Isolation**: The `.razor` file must contain ZERO manual service instantiations (`new Service()`). All services must be injected via `@inject`.

## 2. Data Contracts (DTOs)
- **Strict Typing**: Use dedicated DTOs (e.g., `CategoryUpsertDto`) for Create/Update operations. 
- **Anonymous Objects Prohibited**: No anonymous objects in `GetDto` methods to ensure contract safety with the API.

## 3. Validation Strategy
- **FluentValidation**: Every maintenance page must have an associated `AbstractValidator<T>`.
- **UI Integration**: The `MaintenanceBase` must receive the validator instance and trigger `ValidateAsync` before any API call.

## 4. Service Layer
- **Interface-Based**: Each module must expose its services through interfaces (e.g., `IInventoryService`).
- **Endpoint Encapsulation**: The specific URL/Route must live inside the service implementation, not in the UI code.

## 5. Visual Scannability
- Tables must use `Dense="true"`, `Hover="true"`, and `Striped="true"`.
- Buttons must use standard MudBlazor Icons and consistent Color Schemes (Primary for Edit/Save, Error for Delete).