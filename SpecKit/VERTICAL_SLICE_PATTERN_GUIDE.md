# Vertical Slice Pattern - Quick Reference Card

## Pattern Summary: Service ? MediatR Migration

### Step 1: Create Feature Folder
```
HeuristicLogix.Features/{Schema}/{EntityPlural}/
Example: HeuristicLogix.Features/Inventory/Categories/
Example: HeuristicLogix.Features/Core/UnitOfMeasures/
```

### Step 2: Create Vertical Slices (One per Operation)

#### Query Pattern
```csharp
namespace HeuristicLogix.Features.{Schema}.{EntityPlural};

// Query Record
public sealed record Get{Entity}ByIdQuery(int {Entity}Id) : IRequest<{Entity}?>;

// Handler with Primary Constructor
public sealed class Get{Entity}ByIdHandler(DbContext context) 
    : IRequestHandler<Get{Entity}ByIdQuery, {Entity}?>
{
    public async Task<{Entity}?> Handle(
        Get{Entity}ByIdQuery request, 
        CancellationToken cancellationToken)
    {
        return await context.Set<{Entity}>()
            .FirstOrDefaultAsync(e => e.{Entity}Id == request.{Entity}Id, 
                                cancellationToken);
    }
}
```

#### Command Pattern (with Validation)
```csharp
namespace HeuristicLogix.Features.{Schema}.{EntityPlural};

// Command Record
public sealed record Create{Entity}Command({Entity} {Entity}) : IRequest<{Entity}>;

// Handler with Business Rules
public sealed class Create{Entity}Handler(DbContext context) 
    : IRequestHandler<Create{Entity}Command, {Entity}>
{
    public async Task<{Entity}> Handle(
        Create{Entity}Command request, 
        CancellationToken cancellationToken)
    {
        // Validate business rules
        var exists = await context.Set<{Entity}>()
            .AnyAsync(e => e.UniqueField == request.{Entity}.UniqueField, 
                     cancellationToken);
        
        if (exists)
        {
            throw new InvalidOperationException("Duplicate detected");
        }

        // Persist
        context.Set<{Entity}>().Add(request.{Entity});
        await context.SaveChangesAsync(cancellationToken);
        
        return request.{Entity};
    }
}
```

### Step 3: Refactor Service to Bridge Pattern

```csharp
using MediatR;

public class {Entity}Service : I{Entity}Service
{
    private readonly IMediator _mediator;
    private readonly ILogger<{Entity}Service> _logger;

    // Constructor: Remove DbContext, Add IMediator
    public {Entity}Service(IMediator mediator, ILogger<{Entity}Service> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // Each method delegates to MediatR
    public async Task<{Entity}?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving {Entity} with ID: {Id}", id);
        return await _mediator.Send(new Get{Entity}ByIdQuery(id));
    }

    public async Task<{Entity}> CreateAsync({Entity} entity)
    {
        _logger.LogInformation("Creating {Entity}");
        return await _mediator.Send(new Create{Entity}Command(entity));
    }
}
```

### Step 4: Update Module Registration

**InventoryModuleExtensions.cs**:
```csharp
// OLD: Manual factory with DbContext
services.AddScoped<I{Entity}Service>(sp => 
    new {Entity}Service(
        sp.GetRequiredService<DbContext>(), 
        sp.GetRequiredService<ILogger<{Entity}Service>>()));

// NEW: Simple registration (IMediator auto-resolved)
services.AddScoped<I{Entity}Service, {Entity}Service>();
```

### Step 5: Verify MediatR Registration (One-Time Setup)

**Program.cs** (already configured for all HeuristicLogix.Features):
```csharp
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssemblyContaining<GetCategoriesHandler>();
});
```
? This scans the **entire assembly**, so no additional registration needed for new handlers!

## Checklist for New Entity Migration

- [ ] Create folder: `HeuristicLogix.Features/{Schema}/{EntityPlural}/`
- [ ] Create `Get{EntityPlural}.cs` (Query + Handler)
- [ ] Create `Get{Entity}ById.cs` (Query + Handler)
- [ ] Create `Create{Entity}.cs` (Command + Handler with validation)
- [ ] Create `Update{Entity}.cs` (Command + Handler with validation)
- [ ] Create `Delete{Entity}.cs` (Command + Handler with referential integrity)
- [ ] Create `{Entity}Exists.cs` (Query + Handler) - if needed
- [ ] Create any additional existence checks (e.g., `{Entity}ExistsByName.cs`)
- [ ] Refactor `{Entity}Service.cs` to use `IMediator`
- [ ] Update `InventoryModuleExtensions.cs` to simple registration
- [ ] Build and test
- [ ] Verify `.razor` files work unchanged

## Naming Conventions

| Element | Pattern | Example (Category) | Example (UnitOfMeasure) |
|---------|---------|-------------------|------------------------|
| Namespace | `HeuristicLogix.Features.{Schema}.{EntityPlural}` | `...Inventory.Categories` | `...Core.UnitOfMeasures` |
| Query | `Get{Entity}ByIdQuery` | `GetCategoryByIdQuery` | `GetUnitOfMeasureByIdQuery` |
| Command | `{Verb}{Entity}Command` | `CreateCategoryCommand` | `CreateUnitOfMeasureCommand` |
| Handler | `{Verb}{Entity}Handler` | `CreateCategoryHandler` | `CreateUnitOfMeasureHandler` |
| File | `{Verb}{Entity}.cs` | `CreateCategory.cs` | `CreateUnitOfMeasure.cs` |

## Common Validations

### Duplicate Check (Create)
```csharp
var exists = await context.Set<Entity>()
    .AnyAsync(e => e.UniqueField == request.Entity.UniqueField, 
             cancellationToken);
if (exists) throw new InvalidOperationException("Duplicate");
```

### Duplicate Check (Update - Exclude Self)
```csharp
var duplicate = await context.Set<Entity>()
    .AnyAsync(e => e.UniqueField == request.Entity.UniqueField 
                   && e.Id != request.Entity.Id, 
             cancellationToken);
if (duplicate) throw new InvalidOperationException("Duplicate");
```

### Referential Integrity (Delete)
```csharp
var hasRelated = await context.Set<RelatedEntity>()
    .AnyAsync(r => r.ForeignKeyId == request.EntityId, 
             cancellationToken);
if (hasRelated) throw new InvalidOperationException("Cannot delete - in use");
```

## Benefits of This Pattern

? **Testability**: Handlers can be tested independently  
? **Separation of Concerns**: Each feature is self-contained  
? **Maintainability**: Easy to find and modify specific operations  
? **Scalability**: Adding new features doesn't affect existing ones  
? **No UI Changes**: Bridge pattern preserves existing contracts  
? **Discoverability**: Clear file structure mirrors business operations  

## Migration Progress Tracker

| Entity | Schema | Status | Bridge Service | Handlers |
|--------|--------|--------|---------------|----------|
| **Category** | Inventory | ? Complete | CategoryService | 7 handlers |
| **UnitOfMeasure** | Core | ? Complete | UnitOfMeasureService | 7 handlers |
| Brand | Inventory | ? Pending | BrandService | - |
| Supplier | Purchasing | ? Pending | SupplierService | - |
| TaxConfiguration | Core | ? Pending | TaxConfigurationService | - |

---

**Last Updated**: UnitOfMeasure migration completed  
**Next Target**: Brand (Inventory Schema)
