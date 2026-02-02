# ERP Inventory Specification Kit

## 1. Domain Logic: Multi-Unit Management
All items must be tracked using a **Base Unit of Measure** (the smallest indivisible unit, e.g., "Pala", "Unidad").

### 1.1 Unit Conversion
* **ItemUnitConversions Table:** Defines the relationship between a non-base unit and the base unit.
* **Calculation Rule:** `Quantity in Base Unit = Quantity in Transactional Unit * ConversionFactorQuantity`.
* **Example:** If 1 "Metro" = 40 "Palas", the `ConversionFactorQuantity` is 40.00.

## 2. Data Integrity & Precision
* **Monetary Values:** All prices and costs must use `DECIMAL(18,4)` to prevent rounding errors in high-volume unit sales.
* **Quantities:** Stock quantities must use `DECIMAL(18,2)`.

## 3. Storage Rules
* **Costing:** Use **Weighted Average Cost (WAC)**. This must be recalculated only upon final approval of a Purchase Invoice.
* **Stock Accuracy:** `CurrentStockQuantity` is a read-only field for the UI; it is only updated via formal transactions (Purchases, Sales, Adjustments).