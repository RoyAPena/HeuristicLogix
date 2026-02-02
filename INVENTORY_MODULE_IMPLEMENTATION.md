# ??? Modular Monolith - Inventory Module Implementation

## ? Phase 1: Module Infrastructure - COMPLETE

### Created Files

#### 1. **Module Project**
- `HeuristicLogix.Modules.Inventory\HeuristicLogix.Modules.Inventory.csproj`

#### 2. **Services**
- `Services\ICategoryService.cs` - Category service interface
- `Services\CategoryService.cs` - Category service implementation
- `Services\IUnitOfMeasureService.cs` - UnitOfMeasure service interface
- `Services\UnitOfMeasureService.cs` - UnitOfMeasure service implementation

#### 3. **Validators**
- `Validators\CategoryValidator.cs` - FluentValidation for Category
- `Validators\UnitOfMeasureValidator.cs` - FluentValidation for UnitOfMeasure

#### 4. **Module Registration**
- `InventoryModuleExtensions.cs` - DI registration extension

#### 5. **API Controllers**
- `HeuristicLogix.Api\Controllers\CategoriesController.cs`
- `HeuristicLogix.Api\Controllers\UnitsOfMeasureController.cs`

#### 6. **Program.cs Updated**
- Added `builder.Services.AddInventoryModule();`

---

## ?? Phase 2: Blazor UI - TO IMPLEMENT

### File Structure
```
HeuristicLogix.ERP/
??? Features/
    ??? Inventory/
        ??? Maintenances/
            ??? CategoryPage.razor
            ??? CategoryPage.razor.cs
            ??? UnitOfMeasurePage.razor
            ??? UnitOfMeasurePage.razor.cs
```

### CategoryPage.razor Template

```razor
@page "/inventory/categories"
@inject HttpClient Http
@inject ILogger<CategoryPage> Logger

<PageTitle>Categorías - Mantenimiento</PageTitle>

<div class="container-fluid">
    <div class="row mb-3">
        <div class="col">
            <h1>Mantenimiento de Categorías</h1>
        </div>
        <div class="col-auto">
            <button class="btn btn-primary" @onclick="ShowCreateDialog">
                <i class="bi bi-plus-circle"></i> Nueva Categoría
            </button>
        </div>
    </div>

    @if (isLoading)
    {
        <div class="text-center">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
        </div>
    }
    else if (categories == null || !categories.Any())
    {
        <div class="alert alert-info">
            No hay categorías registradas. Haga clic en "Nueva Categoría" para agregar una.
        </div>
    }
    else
    {
        <div class="table-responsive">
            <table class="table table-striped table-hover">
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Nombre</th>
                        <th class="text-end">Acciones</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var category in categories)
                    {
                        <tr>
                            <td>@category.CategoryId</td>
                            <td>@category.CategoryName</td>
                            <td class="text-end">
                                <button class="btn btn-sm btn-outline-primary me-1" 
                                        @onclick="() => ShowEditDialog(category)">
                                    <i class="bi bi-pencil"></i> Editar
                                </button>
                                <button class="btn btn-sm btn-outline-danger" 
                                        @onclick="() => ShowDeleteConfirmation(category)">
                                    <i class="bi bi-trash"></i> Eliminar
                                </button>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    }
</div>

@* Edit/Create Dialog *@
@if (showDialog)
{
    <div class="modal show d-block" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">@(isEditing ? "Editar Categoría" : "Nueva Categoría")</h5>
                    <button type="button" class="btn-close" @onclick="CloseDialog"></button>
                </div>
                <div class="modal-body">
                    <EditForm Model="currentCategory" OnValidSubmit="SaveCategory">
                        <FluentValidationValidator />
                        <div class="mb-3">
                            <label class="form-label">Nombre de Categoría</label>
                            <InputText class="form-control" @bind-Value="currentCategory.CategoryName" />
                            <ValidationMessage For="() => currentCategory.CategoryName" />
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" @onclick="CloseDialog">Cancelar</button>
                            <button type="submit" class="btn btn-primary">
                                @(isEditing ? "Actualizar" : "Crear")
                            </button>
                        </div>
                    </EditForm>
                </div>
            </div>
        </div>
    </div>
    <div class="modal-backdrop show"></div>
}

@code {
    private List<Category>? categories;
    private Category currentCategory = new Category { CategoryId = 0, CategoryName = string.Empty };
    private bool isLoading = true;
    private bool showDialog = false;
    private bool isEditing = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadCategories();
    }

    private async Task LoadCategories()
    {
        isLoading = true;
        try
        {
            categories = await Http.GetFromJsonAsync<List<Category>>("api/inventory/categories");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading categories");
        }
        finally
        {
            isLoading = false;
        }
    }

    private void ShowCreateDialog()
    {
        currentCategory = new Category { CategoryId = 0, CategoryName = string.Empty };
        isEditing = false;
        showDialog = true;
    }

    private void ShowEditDialog(Category category)
    {
        currentCategory = new Category 
        { 
            CategoryId = category.CategoryId, 
            CategoryName = category.CategoryName 
        };
        isEditing = true;
        showDialog = true;
    }

    private void CloseDialog()
    {
        showDialog = false;
        currentCategory = new Category { CategoryId = 0, CategoryName = string.Empty };
    }

    private async Task SaveCategory()
    {
        try
        {
            if (isEditing)
            {
                await Http.PutAsJsonAsync($"api/inventory/categories/{currentCategory.CategoryId}", currentCategory);
            }
            else
            {
                await Http.PostAsJsonAsync("api/inventory/categories", currentCategory);
            }

            await LoadCategories();
            CloseDialog();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving category");
        }
    }

    private async Task ShowDeleteConfirmation(Category category)
    {
        // Implement confirmation dialog
        // Then call DeleteCategory
    }

    private async Task DeleteCategory(int categoryId)
    {
        try
        {
            await Http.DeleteAsync($"api/inventory/categories/{categoryId}");
            await LoadCategories();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting category");
        }
    }
}
```

### UnitOfMeasurePage.razor Template

```razor
@page "/inventory/units-of-measure"
@inject HttpClient Http
@inject ILogger<UnitOfMeasurePage> Logger

<PageTitle>Unidades de Medida - Mantenimiento</PageTitle>

<div class="container-fluid">
    <div class="row mb-3">
        <div class="col">
            <h1>Mantenimiento de Unidades de Medida</h1>
        </div>
        <div class="col-auto">
            <button class="btn btn-primary" @onclick="ShowCreateDialog">
                <i class="bi bi-plus-circle"></i> Nueva Unidad
            </button>
        </div>
    </div>

    @if (isLoading)
    {
        <div class="text-center">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
        </div>
    }
    else if (units == null || !units.Any())
    {
        <div class="alert alert-info">
            No hay unidades de medida registradas.
        </div>
    }
    else
    {
        <div class="table-responsive">
            <table class="table table-striped table-hover">
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Nombre</th>
                        <th>Símbolo</th>
                        <th class="text-end">Acciones</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var unit in units)
                    {
                        <tr>
                            <td>@unit.UnitOfMeasureId</td>
                            <td>@unit.UnitOfMeasureName</td>
                            <td><span class="badge bg-secondary">@unit.UnitOfMeasureSymbol</span></td>
                            <td class="text-end">
                                <button class="btn btn-sm btn-outline-primary me-1" 
                                        @onclick="() => ShowEditDialog(unit)">
                                    <i class="bi bi-pencil"></i> Editar
                                </button>
                                <button class="btn btn-sm btn-outline-danger" 
                                        @onclick="() => ShowDeleteConfirmation(unit)">
                                    <i class="bi bi-trash"></i> Eliminar
                                </button>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    }
</div>

@* Dialog similar to CategoryPage *@

@code {
    private List<UnitOfMeasure>? units;
    private UnitOfMeasure currentUnit = new();
    private bool isLoading = true;
    private bool showDialog = false;
    private bool isEditing = false;

    // Similar implementation to CategoryPage
}
```

---

## ?? API Project Configuration

### Add Project Reference

In `HeuristicLogix.Api.csproj`:
```xml
<ItemGroup>
  <ProjectReference Include="..\HeuristicLogix.Modules.Inventory\HeuristicLogix.Modules.Inventory.csproj" />
</ItemGroup>
```

---

## ?? Testing the Module

### 1. **Build Solution**
```powershell
dotnet build
```

### 2. **Run API**
```powershell
cd HeuristicLogix.Api
dotnet run
```

### 3. **Test Endpoints**

**Get all categories:**
```bash
curl http://localhost:5000/api/inventory/categories
```

**Create category:**
```bash
curl -X POST http://localhost:5000/api/inventory/categories \
  -H "Content-Type: application/json" \
  -d '{"categoryId": 0, "categoryName": "Electronics"}'
```

**Get all units of measure:**
```bash
curl http://localhost:5000/api/inventory/unitsofmeasure
```

**Create unit of measure:**
```bash
curl -X POST http://localhost:5000/api/inventory/unitsofmeasure \
  -H "Content-Type: application/json" \
  -d '{"unitOfMeasureId": 0, "unitOfMeasureName": "Kilogram", "unitOfMeasureSymbol": "kg"}'
```

---

## ? Module Features Implemented

### Service Layer
- [x] ICategoryService with CRUD operations
- [x] IUnitOfMeasureService with CRUD operations
- [x] Duplicate name/symbol validation
- [x] Foreign key constraint checking on delete
- [x] Comprehensive logging

### Validation
- [x] FluentValidation for Category
- [x] FluentValidation for UnitOfMeasure
- [x] Client-side and server-side validation

### API Layer
- [x] RESTful Categories controller
- [x] RESTful UnitsOfMeasure controller
- [x] Proper HTTP status codes
- [x] Error handling and logging
- [x] OpenAPI/Swagger documentation

### Architecture
- [x] Modular monolith structure
- [x] Clean separation of concerns
- [x] Easy to extract into microservice
- [x] Dependency injection
- [x] Hybrid ID architecture (int IDs)

---

## ?? Next Steps

1. **Create Blazor UI pages** using the templates above
2. **Add navigation** to access maintenance pages
3. **Implement confirmation dialogs** for delete operations
4. **Add search/filter** capabilities
5. **Implement pagination** for large datasets
6. **Add export to Excel** functionality
7. **Create unit tests** for services and validators

---

## ?? Project Structure

```
HeuristicLogix.Modules.Inventory/
??? Services/
?   ??? ICategoryService.cs
?   ??? CategoryService.cs
?   ??? IUnitOfMeasureService.cs
?   ??? UnitOfMeasureService.cs
??? Validators/
?   ??? CategoryValidator.cs
?   ??? UnitOfMeasureValidator.cs
??? InventoryModuleExtensions.cs
??? HeuristicLogix.Modules.Inventory.csproj

HeuristicLogix.Api/
??? Controllers/
    ??? CategoriesController.cs
    ??? UnitsOfMeasureController.cs
```

---

## ?? Module Complete!

The Inventory module is now:
- ? Fully decoupled from the main API
- ? Self-contained with its own services and validators
- ? Easily extractable into a microservice
- ? Following clean architecture principles
- ? Ready for Blazor UI integration

**Status:** Backend infrastructure complete, Blazor UI templates provided
