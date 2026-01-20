\# Logistics Intelligence \& Heuristic Capacity



\## Core Concept

Capacity is not strictly volumetric (m3). It is a \*\*Heuristic Score\*\* (0-100) derived from:

\- \*\*Historical Loading:\*\* What has been successfully loaded before.

\- \*\*Expert Assignment:\*\* Manual overrides by the expert user.

\- \*\*Compatibility:\*\* Learned material relationships (to be defined in Phase 2).



\## Data Telemetry Rules

Every manual assignment must capture:

1\. \*\*Selection Time:\*\* Time elapsed between displaying an order and the expert's assignment.

2\. \*\*Override Context:\*\* If the AI suggested Truck A but the expert chose Truck B, the system must log the `OverrideReasonTag`.



\## Technical Strictness

\- All DTOs must use `explicit typing`.

\- Use `required` and `init` for data integrity.

\- No business logic should be hardcoded in the UI; use Service-based architecture.

