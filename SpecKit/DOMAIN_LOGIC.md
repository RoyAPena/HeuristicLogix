\# Domain \& Business Logic - HeuristicLogix



\## 1. Core Entities

\- \*\*Conduce (Delivery Order):\*\*

&nbsp;   - Properties: Id, ClientName, RawAddress, Latitude, Longitude, Status (Pending, Scheduled, OutForDelivery, Completed).

&nbsp;   - Features: Itemized list with quantities.

\- \*\*Truck:\*\*

&nbsp;   - Properties: Id, PlateNumber, TruckType (Flatbed, Dump, Crane).

&nbsp;   - Heuristic Capacity: Capacity is NOT defined by rigid 3D dimensions but is learned from expert assignment history.



\## 2. The Heuristic Engine

\- \*\*Compatibility Matrix:\*\* A dynamic ruleset defining material exclusions (e.g., "Cannot load Rebar on top of Cement bags").

\- \*\*ETA Logic:\*\* `TotalTime = GoogleTrafficTime + ServiceTime\_Inference`.

\- \*\*Manual Pinning:\*\* If Google Geocoding accuracy is low, the UI must force the user to manually drop a pin on the map.

