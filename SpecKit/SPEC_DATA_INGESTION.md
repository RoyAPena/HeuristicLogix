\# SPECKIT\_DATA\_INGESTION.md



\## 1. STRATEGY \& BOUNDARIES

\- \*\*Domain:\*\* Logistics / Inventory

\- \*\*Objective:\*\* Deterministic data capture for ERP training and operations.

\- \*\*Modes:\*\* - Bulk Ingestion (Excel/CSV)

&nbsp;   - Manual Ingestion (Blazor Fast-Entry)

\- \*\*Status:\*\* Shadow Mode (Parallel Execution)



\## 2. DATA CONTRACT (ENTITY\_MAP)

| Field | Type | Required | Constraint |

| :--- | :--- | :--- | :--- |

| Fecha | DateTime | Yes | ISO 8601 |

| ClienteNombre | String | Yes | MaxLength(200) |

| ProductoDescripcion | String | Yes | Raw Text Input |

| Cantidad | Decimal | Yes | Precision(18,2) |

| UnidadMedida | String | Yes | {Metro, Saco, Pala, Global} |

| CamionPlaca | String | Yes | Regex(PlateFormat) |

| DecisionExperto | String | No | MaxLength(1000) |



\## 3. HUMAN-IN-THE-LOOP (HIL) FLOW

1\. \*\*Discovery:\*\* Parser identifies unique `ProductoDescripcion` + `UnidadMedida`.

2\. \*\*Pending State:\*\* New entries are persisted in `ProductTaxonomy` with `IsVerifiedByExpert = false`.

3\. \*\*Data Tagging:\*\* All related `ConduceCreatedEvent` must include `HighUncertainty: true` until verified.

4\. \*\*Expert Action:\*\* UI must allow manual override of `WeightFactor` and `Category`.



\## 4. TECHNICAL IMPLEMENTATION (LOGISTICS MODULE)

\- \*\*Library:\*\* MiniExcel (Streaming approach).

\- \*\*Service:\*\* `ExcelIngestionService.cs`

\- \*\*Pattern:\*\* Transactional Outbox.

\- \*\*Standard:\*\* Explicit Typing, No `var`, Async/Await.



\## 5. EVENT SCHEMA (KAFKA)

\- \*\*Topic:\*\* `logistics.conduce.created`

\- \*\*Header:\*\* `is\_historic: true`

\- \*\*Payload:\*\*

&nbsp;   ```json

&nbsp;   {

&nbsp;     "eventId": "Guid",

&nbsp;     "timestamp": "DateTime",

&nbsp;     "data": {

&nbsp;       "client": "string",

&nbsp;       "description": "string",

&nbsp;       "quantity": "decimal",

&nbsp;       "unit": "string",

&nbsp;       "uncertainty": "bool"

&nbsp;     }

&nbsp;   }

&nbsp;   ```

