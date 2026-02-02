# ERP Purchasing & Fiscal Specification Kit

## 1. National Tax Compliance (DGII)
* **Identification:** `NationalTaxIdentificationNumber` must be validated for 9 or 11 digits (Dominican RNC).
* **Receipts:** `FiscalReceiptNumber` (NCF) must follow the structure: B + Type (2) + Sequence (8).

## 2. Mass Ingestion Pattern (Staging Area)
To maintain performance and allow human review, the purchasing flow is split:

### Phase A: Ingestion (Staging)
* Data is written to `StagingPurchaseInvoices` and `StagingPurchaseInvoiceDetails`.
* No inventory or cost tables are affected.
* Minimal locking on main tables.

### Phase B: Validation & Approval
* A domain service validates the staging data against `Inventory.Items` and `Core.TaxConfigurations`.
* **Atomic Transaction:** Upon approval, a single SQL transaction must:
    1. Move data to `PurchaseInvoices`.
    2. Update `Inventory.Items.CurrentStockQuantity`.
    3. Recalculate `Inventory.Items.CostPricePerBaseUnit`.
    4. Update `Purchasing.ItemSuppliers` catalog with the latest price and date.

## 3. Supplier Relations
* **Credit Terms:** `DefaultCreditDaysDuration` is a template. The real due date is calculated per invoice: `InvoiceIssueDateTime + CreditDays`.