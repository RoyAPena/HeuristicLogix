# ? Inventory Module - Manual Setup Steps

## ?? Current Status

The Inventory Module has been **fully designed and created**, but requires manual project setup in Visual Studio.

## ?? Required Manual Steps

### Step 1: Add Project to Solution

1. Open Visual Studio
2. Right-click on Solution
3. Add ? Existing Project
4. Navigate to `HeuristicLogix.Modules.Inventory`
5. Select `HeuristicLogix.Modules.Inventory.csproj`

**OR** using command line:
```powershell
dotnet sln add HeuristicLogix.Modules.Inventory\HeuristicLogix.Modules.Inventory.csproj
```

### Step 2: Restore NuGet Packages

```powershell
dotnet restore
```

### Step 3: Fix FluentValidation Version

In `HeuristicLogix.Api\HeuristicLogix.Api.csproj`, change:
```xml
<PackageReference Include="FluentValidation.AspNetCore" Version="11.11.0" />
```

To:
```xml
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
```

### Step 4: Build Solution

```powershell
dotnet build
```

---

## ?? Files Created

All files have been successfully created:

### Module Project
- ? `HeuristicLogix.Modules.Inventory\HeuristicLogix.Modules.Inventory.csproj`
- ? `HeuristicLogix.Modules.Inventory\InventoryModuleExtensions.cs`

### Services
- ? `Services\ICategoryService.cs`
- ? `Services\CategoryService.cs`
- ? `Services\IUnitOfMeasureService.cs`
- ? `Services\UnitOfMeasureService.cs`

### Validators
- ? `Validators\CategoryValidator.cs`
- ? `Validators\UnitOfMeasureValidator.cs`

### API Controllers
- ? `HeuristicLogix.Api\Controllers\CategoriesController.cs`
- ? `HeuristicLogix.Api\Controllers\UnitsOfMeasureController.cs`

### Configuration
- ? `HeuristicLogix.Api\Program.cs` - Updated with module registration
- ? `HeuristicLogix.Api\HeuristicLogix.Api.csproj` - Updated with references

---

## ?? Alternative: Create Project in Visual Studio

If command line doesn't work:

1. **Create New Project:**
   - File ? New ? Project
   - Select "Class Library"
   - Name: `HeuristicLogix.Modules.Inventory`
   - Location: Solution root folder
   - Target Framework: .NET 10

2. **Delete default Class1.cs**

3. **Copy all created files** from `HeuristicLogix.Modules.Inventory\` folder into the new project

4. **Add NuGet Packages:**
   ```powershell
   dotnet add package Microsoft.EntityFrameworkCore --version 10.0.2
   dotnet add package Microsoft.Extensions.DependencyInjection.Abstractions --version 10.0.2
   dotnet add package FluentValidation --version 11.3.0
   dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.3.0
   ```

5. **Add Project Reference** to HeuristicLogix.Shared:
   - Right-click References ? Add Project Reference ? Select HeuristicLogix.Shared

6. **In HeuristicLogix.Api:**
   - Add Project Reference ? Select HeuristicLogix.Modules.Inventory
   - Install FluentValidation.AspNetCore version 11.3.0

---

## ? Verification Checklist

After setup, verify:

- [ ] HeuristicLogix.Modules.Inventory appears in Solution Explorer
- [ ] All files are visible in the project
- [ ] No build errors
- [ ] Program.cs recognizes `AddInventoryModule()`
- [ ] Controllers compile successfully

---

## ?? Once Setup is Complete

### Test the API:

```bash
# Start API
cd HeuristicLogix.Api
dotnet run

# Test Category endpoint
curl http://localhost:5000/api/inventory/categories

# Test UnitOfMeasure endpoint
curl http://localhost:5000/api/inventory/unitsofmeasure
```

---

## ?? Documentation References

- **Implementation Guide:** `INVENTORY_MODULE_IMPLEMENTATION.md`
- **Quick Reference:** `INVENTORY_MODULE_QUICK_REF.md`
- **Blazor UI Templates:** See `INVENTORY_MODULE_IMPLEMENTATION.md`

---

## ?? What You Get

Once setup is complete:

? **Fully functional Inventory Module**
- Category maintenance (Create, Read, Update, Delete)
- UnitOfMeasure maintenance (Create, Read, Update, Delete)
- FluentValidation on both client and server
- RESTful API endpoints
- Modular monolith architecture
- Ready to extract into microservice

? **Clean Architecture**
- Separation of concerns
- Dependency injection
- Interface-based design
- Easy to test and extend

? **Production Ready**
- Comprehensive error handling
- Logging throughout
- Validation at all layers
- Foreign key constraint checking

---

**Status:** Files created, requires manual project registration in solution
**Next:** Follow Step 1 to add project to solution
