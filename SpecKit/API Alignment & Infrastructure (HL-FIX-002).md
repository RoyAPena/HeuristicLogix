# SPECKIT: API ALIGNMENT & INFRASTRUCTURE
# Version: 1.0 (New)
# Target: HTTP/REST Communication Standards

[COMMUNICATION_PROTOCOL]
- [cite_start]BASE_PORT: 7086 (Critical) [cite: 42]
- ADDRESS: https://localhost:7086/
- ENDPOINT_PATTERN: api/inventory/{controller}/{id?}

[HTTP_VERBS_MAPPING]
- [cite_start]GET: Fetch collection or single item [cite: 29]
- [cite_start]POST: Create new entity [cite: 41]
- [cite_start]PUT: Update entity with ID in URL [cite: 40]
- [cite_start]DELETE: Remove entity with ID in URL [cite: 48]

[ERROR_RESPONSE_CONTRACT]
- [cite_start]FORMAT: Expect JSON object: { "message": "string" } 
- [cite_start]LOGIC: HandleHttpError must attempt to read this JSON body when StatusCode is 400 (BadRequest) [cite: 53]
- [cite_start]FEEDBACK: Instead of generic "Error de validación", the Snackbar must display the specific server-side message (e.g., Foreign Key constraints) [cite: 51, 57]

[SERVICE_ADAPTER_CONTRACT]
- [cite_start]WRAPPER: BaseMaintenanceServiceAdapter must wrap ISpecificMaintenanceService [cite: 68]
- [cite_start]IMPLEMENTATION: BaseHttpMaintenanceService (HttpClient) must explicitly append ID to URL for PUT/DELETE [cite: 40, 48]