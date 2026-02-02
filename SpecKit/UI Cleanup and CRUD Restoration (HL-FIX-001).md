# SPECKIT: UI CLEANUP & CRUD FUNCTIONALITY
# Version: 1.1

[UI_REFINEMENT]
- TARGET: Sidebar (MudNavMenu).
- ACTION: Remove all navigation links EXCEPT 'Categorías' and 'Unidades de Medida'.
- CONSISTENCY: Ensure 'Unidades de Medida' uses Icons.Material.Filled.Straighten.

[CRUD_REPAIR_LOGIC]
- API_BASE_URL: http://localhost:7086 (Critical).
- SERVICE_ADAPTER_FIX: 
    - Verify that `BaseMaintenanceServiceAdapter` uses the correct HTTP Verbs:
        - Create: POST
        - Update: PUT (passing ID in URL and DTO in Body)
        - Delete: DELETE (passing ID in URL)
- SERIALIZATION: Ensure the DTOs match the API expectations (JsonSerializerOptions.PropertyNameCaseInsensitive).

[MAINTENANCE_BASE_RECOVERY]
- Ensure 'SaveItem' and 'DeleteItem' in MaintenanceBase.razor are correctly awaiting the Service calls.
- Verify 'LoadItems' is called after every successful Mutation (Create/Update/Delete).