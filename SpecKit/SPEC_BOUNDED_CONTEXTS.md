# HeuristicLogix: Bounded Contexts & Modular Architecture (2026 v2)

## 1. Logistics (Core)
- **Key Flow:** Requests "Credit Clearance" from Finance -> Creates `Conduce` -> Emits `ConduceFinalized`.
- **AI Hook:** Gemini 2.5 Flash reviews `ExpertDecision` when suggestions are ignored.

## 2. Inventory (Operational)
- **Strategy:** Event Sourcing.
- **Reaction:** Listens to `ConduceFinalized` to commit `StockMovement`.
- **Constraint:** If `StockMovement` fails (insufficient), emits `InventoryShortage` to alert Logistics/Father.

## 3. Finance (Support)
- **Responsibility:** Credit scoring and AR.
- **Service:** Provides a synchronous `IsClientEligible(clientId)` method for Logistics to call during checkout (Safety first).
- **Notification:** Emits `PaymentReceived` to potentially unblock pending deliveries.

## 4. Procurement (Resource)
- **AI Integration:** GPT-5.2 analyzes `StockMovement` trends weekly to suggest `PurchaseOrder` drafts to Providers.