\# Architecture Specification - HeuristicLogix



\## 1. Technical Foundation

\- \*\*Solution Name:\*\* HeuristicLogix

\- \*\*Framework:\*\* .NET 10 (C# 14)

\- \*\*Frontend:\*\* Blazor WebAssembly Standalone

\- \*\*UI Library:\*\* MudBlazor (Open Source)

\- \*\*Architecture Pattern:\*\* Vertical Slice Architecture

\- \*\*External APIs:\*\* Google Maps Platform (Routes \& Geocoding)

\- \*\*Database:\*\* Azure SQL with Entity Framework Core



\## 2. Development Standards

\- \*\*Modern C#:\*\* Use Primary Constructors, Collection Expressions, and Required Members.

\- \*\*Geospatial Constraint:\*\* No 'Conduce' can be persisted without validated Latitude/Longitude coordinates.

\- \*\*AI Data Readiness:\*\* Schema must include `AIPredictedTime`, `ExpertDecisionTime`, and `ActualServiceTime` for future ML training.

## Data Serialization & Persistence Standards

### Enum Handling
- **Strict Requirement:** All Enums must be treated and persisted as **Strings**, never as Integers.
- **Implementation:** - For JSON serialization (API/Client): Use `JsonStringEnumConverter`.
  - For Database persistence: Ensure the mapping layer (EF Core or Dapper) stores the string representation.
- **Reasoning:** To prevent data corruption and confusion during ML training phases and to ensure historical data remains valid even if Enum integer values are reordered or added.
