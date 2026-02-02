# ERP Relationship Mapping - Quick Reference

## ?? Relationship Pattern Template

### Every FK Needs THREE Things:

1. **FK Property** in entity class
2. **Navigation Property** in entity class  
3. **Explicit Mapping** in AppDbContext

```csharp
// 1. FK Property
public int CategoryId { get; init; }

// 2. Navigation Property
public virtual Category? Category { get; init; }

// 3. Explicit Mapping (in AppDbContext)
entity.HasOne(e => e.Category)
    .WithMany()
    .HasForeignKey(e => e.CategoryId)
    .OnDelete(DeleteBehavior.Restrict)
    .IsRequired();
```

## ?? Complete Relationship Matrix

| Entity | Has FK To | FK Type | DeleteBehavior | Navigation Property |
|--------|-----------|---------|----------------|---------------------|
| **Item** | Brand | int (nullable) | Restrict | `Brand?` |
| **Item** | Category | int | Restrict | `Category?` |
| **Item** | TaxConfiguration | **Guid** | Restrict | `TaxConfiguration?` |
| **Item** | BaseUnitOfMeasure | int | Restrict | `BaseUnitOfMeasure?` |
| **Item** | DefaultSalesUnitOfMeasure | int (nullable) | Restrict | `DefaultSalesUnitOfMeasure?` |
| **ItemUnitConversion** | Item | int | **Cascade** | `Item?` |
| **ItemUnitConversion** | FromUnitOfMeasure | int | Restrict | `FromUnitOfMeasure?` |
| **ItemUnitConversion** | ToUnitOfMeasure | int | Restrict | `ToUnitOfMeasure?` |
| **ItemSupplier** | Item | int | Restrict | `Item?` |
| **ItemSupplier** | Supplier | **Guid** | Restrict | `Supplier?` |
| **StagingPurchaseInvoice** | Supplier | **Guid** | Restrict | `Supplier?` |
| **StagingPurchaseInvoiceDetail** | StagingPurchaseInvoice | **Guid** | **Cascade** | `StagingPurchaseInvoice?` |
| **StagingPurchaseInvoiceDetail** | Item | int | Restrict | `Item?` |

## ?? Code Templates

### Template 1: Simple Required FK (int)
```csharp
// Entity
public class Item
{
    public int CategoryId { get; init; }
    public virtual Category? Category { get; init; }
}

// AppDbContext
entity.HasOne(e => e.Category)
    .WithMany()
    .HasForeignKey(e => e.CategoryId)
    .OnDelete(DeleteBehavior.Restrict)
    .IsRequired();
```

### Template 2: Nullable FK (int)
```csharp
// Entity
public class Item
{
    public int? BrandId { get; init; }
    public virtual Brand? Brand { get; init; }
}

// AppDbContext
entity.HasOne(e => e.Brand)
    .WithMany()
    .HasForeignKey(e => e.BrandId)
    .OnDelete(DeleteBehavior.Restrict)
    .IsRequired(false);
```

### Template 3: Guid FK (Cross-Schema)
```csharp
// Entity
public class Item
{
    public Guid TaxConfigurationId { get; init; }
    public virtual TaxConfiguration? TaxConfiguration { get; init; }
}

// AppDbContext
entity.HasOne(e => e.TaxConfiguration)
    .WithMany()
    .HasForeignKey(e => e.TaxConfigurationId)
    .OnDelete(DeleteBehavior.Restrict)
    .IsRequired();
```

### Template 4: Multiple FKs to Same Table
```csharp
// Entity
public class Item
{
    public int BaseUnitOfMeasureId { get; init; }
    public int? DefaultSalesUnitOfMeasureId { get; init; }
    
    // MUST have distinct navigation names
    public virtual UnitOfMeasure? BaseUnitOfMeasure { get; init; }
    public virtual UnitOfMeasure? DefaultSalesUnitOfMeasure { get; init; }
}

// AppDbContext
entity.HasOne(e => e.BaseUnitOfMeasure)
    .WithMany()
    .HasForeignKey(e => e.BaseUnitOfMeasureId)
    .OnDelete(DeleteBehavior.Restrict)
    .IsRequired();

entity.HasOne(e => e.DefaultSalesUnitOfMeasure)
    .WithMany()
    .HasForeignKey(e => e.DefaultSalesUnitOfMeasureId)
    .OnDelete(DeleteBehavior.Restrict)
    .IsRequired(false);
```

### Template 5: Cascade Delete (Parent-Child)
```csharp
// Entity
public class ItemUnitConversion
{
    public int ItemId { get; init; }
    public virtual Item? Item { get; init; }
}

// AppDbContext
entity.HasOne(e => e.Item)
    .WithMany()
    .HasForeignKey(e => e.ItemId)
    .OnDelete(DeleteBehavior.Cascade) // ?? Cascade
    .IsRequired();
```

### Template 6: One-to-Many with Collection
```csharp
// Parent Entity
public class StagingPurchaseInvoice
{
    public Guid StagingPurchaseInvoiceId { get; init; }
    public ICollection<StagingPurchaseInvoiceDetail> Details { get; init; } = new List<>();
}

// Child Entity
public class StagingPurchaseInvoiceDetail
{
    public Guid StagingPurchaseInvoiceId { get; init; }
    public virtual StagingPurchaseInvoice? StagingPurchaseInvoice { get; init; }
}

// AppDbContext (in Parent configuration)
entity.HasMany(e => e.Details)
    .WithOne(d => d.StagingPurchaseInvoice)
    .HasForeignKey(d => d.StagingPurchaseInvoiceId)
    .OnDelete(DeleteBehavior.Cascade)
    .IsRequired();
```

## ?? DeleteBehavior Decision Tree

```
Is the child entity meaningless without the parent?
?
?? YES ? Use Cascade
?  Examples:
?  - ItemUnitConversion ? Item
?  - StagingPurchaseInvoiceDetail ? StagingPurchaseInvoice
?
?? NO ? Use Restrict
   Examples:
   - Item ? Category (Items exist independently)
   - Item ? Brand (Items exist independently)
   - ItemSupplier ? Item (Junction table - protect both sides)
```

## ?? Common Patterns

### Pattern: Master Data Reference
**When:** FK points to lookup/master table (Category, Brand, UnitOfMeasure, Supplier)  
**DeleteBehavior:** `Restrict`  
**Nullable:** Depends on business rules  

```csharp
entity.HasOne(e => e.Category)
    .WithMany()
    .HasForeignKey(e => e.CategoryId)
    .OnDelete(DeleteBehavior.Restrict)
    .IsRequired();
```

### Pattern: Dependent Child Entity
**When:** Child can't exist without parent (detail records, line items)  
**DeleteBehavior:** `Cascade`  
**Nullable:** Never (always required)  

```csharp
entity.HasOne(e => e.Item)
    .WithMany()
    .HasForeignKey(e => e.ItemId)
    .OnDelete(DeleteBehavior.Cascade)
    .IsRequired();
```

### Pattern: Junction Table (Many-to-Many)
**When:** Composite PK represents relationship  
**DeleteBehavior:** `Restrict` (protect both sides)  
**Nullable:** Never (both required)  

```csharp
// Both FKs use Restrict
entity.HasOne(e => e.Item)
    .WithMany()
    .HasForeignKey(e => e.ItemId)
    .OnDelete(DeleteBehavior.Restrict)
    .IsRequired();

entity.HasOne(e => e.Supplier)
    .WithMany()
    .HasForeignKey(e => e.SupplierId)
    .OnDelete(DeleteBehavior.Restrict)
    .IsRequired();
```

## ?? Query Examples

### Load Single Related Entity
```csharp
var item = await context.Items
    .Include(i => i.Category)
    .FirstOrDefaultAsync(i => i.ItemId == 123);

Console.WriteLine(item.Category?.CategoryName);
```

### Load Multiple Related Entities
```csharp
var item = await context.Items
    .Include(i => i.Category)
    .Include(i => i.Brand)
    .Include(i => i.BaseUnitOfMeasure)
    .FirstOrDefaultAsync(i => i.ItemId == 123);
```

### Load Collection with Related Entities
```csharp
var invoice = await context.StagingPurchaseInvoices
    .Include(i => i.Supplier)
    .Include(i => i.Details)
        .ThenInclude(d => d.Item)
    .FirstOrDefaultAsync(i => i.StagingPurchaseInvoiceId == id);
```

### Projection with Navigation Properties
```csharp
var itemDtos = await context.Items
    .Select(i => new ItemDto
    {
        ItemId = i.ItemId,
        CategoryName = i.Category!.CategoryName,
        BrandName = i.Brand != null ? i.Brand.BrandName : "N/A",
        UnitSymbol = i.BaseUnitOfMeasure!.UnitOfMeasureSymbol
    })
    .ToListAsync();
```

## ?? Common Mistakes

### ? Mistake 1: No Navigation Property
```csharp
// WRONG: Only FK property, no navigation
public class Item
{
    public int CategoryId { get; init; }
    // Missing: public virtual Category? Category { get; init; }
}
```

**Fix:** Always add navigation property

### ? Mistake 2: No Explicit Mapping
```csharp
// WRONG: Only property configuration
entity.Property(e => e.CategoryId).IsRequired();
// Missing: entity.HasOne(e => e.Category)...
```

**Fix:** Always use `HasOne().WithMany()` for FKs

### ? Mistake 3: Wrong DeleteBehavior
```csharp
// WRONG: Cascade delete on master data
entity.HasOne(e => e.Category)
    .WithMany()
    .HasForeignKey(e => e.CategoryId)
    .OnDelete(DeleteBehavior.Cascade); // ? Should be Restrict
```

**Fix:** Use Restrict for master data

### ? Mistake 4: Forgetting IsRequired for Nullable FKs
```csharp
// WRONG: Nullable FK without IsRequired(false)
entity.HasOne(e => e.Brand)
    .WithMany()
    .HasForeignKey(e => e.BrandId)
    .OnDelete(DeleteBehavior.Restrict);
    // Missing: .IsRequired(false)
```

**Fix:** Add `.IsRequired(false)` for nullable FKs

### ? Mistake 5: Not Making Navigation Virtual
```csharp
// WRONG: Not virtual
public Category? Category { get; init; }
```

**Fix:** Always use `virtual` for navigations
```csharp
// CORRECT
public virtual Category? Category { get; init; }
```

## ?? Testing Checklist

- [ ] FK constraint prevents deleting referenced entity (Restrict)
- [ ] FK constraint cascades delete to children (Cascade)
- [ ] Can load entity with `.Include()`
- [ ] Can access navigation property after Include
- [ ] Cannot save entity with invalid FK value
- [ ] Nullable FK accepts null
- [ ] Required FK rejects null

## ?? SQL Generated

### FK Constraint (Restrict)
```sql
ALTER TABLE [Inventory].[Items]
ADD CONSTRAINT [FK_Items_Categories_CategoryId]
FOREIGN KEY ([CategoryId])
REFERENCES [Inventory].[Categories] ([CategoryId])
ON DELETE NO ACTION;
```

### FK Constraint (Cascade)
```sql
ALTER TABLE [Inventory].[ItemUnitConversions]
ADD CONSTRAINT [FK_ItemUnitConversions_Items_ItemId]
FOREIGN KEY ([ItemId])
REFERENCES [Inventory].[Items] ([ItemId])
ON DELETE CASCADE;
```

### Index on FK Column
```sql
CREATE INDEX [IX_Items_CategoryId] 
ON [Inventory].[Items] ([CategoryId]);
```

## ?? Quick Validation

**Before generating migration, verify:**
1. ? All FK properties have navigation properties
2. ? All navigations are `virtual`
3. ? All FK relationships have `HasOne().WithMany()` in DbContext
4. ? DeleteBehavior is appropriate (Restrict for master data, Cascade for children)
5. ? `.IsRequired()` or `.IsRequired(false)` matches FK nullability

**After migration:**
1. ? SQL includes `CONSTRAINT` statements
2. ? Indexes created on FK columns
3. ? Cannot delete referenced entities (test Restrict)
4. ? Cascade deletes work (test Cascade)
5. ? `.Include()` works in queries

---
**Relationship Mapping Complete** ?  
**All FKs Explicitly Mapped** ?  
**Navigation Properties Enabled** ?  
**Ready for Migration** ?
