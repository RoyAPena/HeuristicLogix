# HL-UI-001 Quick Reference Guide

## ?? Color Palette
```css
Primary (Deep Steel Blue):    #283593
Secondary (Alert Amber):      #FF8F00
App Bar (Midnight Navy):      #1A237E
Nav Drawer (Technical Grey):  #F8F9FA
Success:                      #2E7D32
Error:                        #D32F2F
Background:                   #F0F2F5
Surface:                      #FFFFFF
```

## ?? Component Usage

### Buttons
```razor
<!-- Primary Action (Most important action in view) -->
<MudButton Variant="Variant.Filled" Color="Color.Secondary">
    Create New
</MudButton>

<!-- Secondary Action -->
<MudButton Variant="Variant.Filled" Color="Color.Primary">
    Save
</MudButton>

<!-- Tertiary Action -->
<MudButton Variant="Variant.Outlined" Color="Color.Primary">
    Cancel
</MudButton>
```

### Form Inputs
```razor
<!-- Use HeuristicLogix components for automatic styling -->
<HLTextField @bind-Value="model.Name" Label="Name" />
<HLSelect @bind-Value="model.CategoryId" Label="Category">
    @* options *@
</HLSelect>
<HLAutocomplete @bind-Value="model.Item" Label="Search" />
```

### Tables
```razor
<!-- Option 1: Use HLTable wrapper -->
<HLTable Items="@entities">
    <HeaderContent>
        <MudTh>ID</MudTh>
        <MudTh>Name</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd Class="data-id">@context.Id</MudTd>
        <MudTd>@context.Name</MudTd>
    </RowTemplate>
</HLTable>

<!-- Option 2: Standard MudTable with HL-UI-001 props -->
<MudTable Items="@entities"
          Dense="true"
          Hover="true"
          Striped="true"
          FixedHeader="true"
          Elevation="0">
    @* ... *@
</MudTable>
```

## ??? Data Type CSS Classes

Apply to `MudTd` for monospace formatting:

```razor
<MudTd Class="data-id">12345</MudTd>        <!-- Entity IDs -->
<MudTd Class="data-sku">SKU-001</MudTd>     <!-- Product SKUs -->
<MudTd Class="data-price">$99.99</MudTd>    <!-- Prices -->
<MudTd Class="data-code">REF-ABC</MudTd>    <!-- Codes -->
```

## ?? Status Badges

```razor
<span class="status-badge status-success">Activo</span>
<span class="status-badge status-error">Inactivo</span>
<span class="status-badge status-warning">Pendiente</span>
```

## ?? Layout Standards

### Containers
```razor
<MudContainer MaxWidth="MaxWidth.ExtraLarge" Class="mt-4">
    @* Content *@
</MudContainer>
```

### Cards
```razor
<MudCard Elevation="0">  <!-- Always use Elevation="0" -->
    <MudCardHeader>
        @* Header content *@
    </MudCardHeader>
    <MudCardContent>
        @* Main content *@
    </MudCardContent>
</MudCard>
```

## ?? Navigation Structure

Current modules in sidebar:
- **Dashboard** - Main overview
- **Inventario** - Categories, Units, Items, Brands
- **Compras** - Suppliers, Orders, Invoices
- **Ventas** - Customers, Quotes, Orders, Invoices
- **Logística** - Planning, Conduces, Trucks
- **Configuración** - System settings

## ?? MaintenanceBase Usage

All maintenance pages automatically get HL-UI-001 styling:

```razor
<MaintenanceBase TEntity="Category"
                 TDto="CategoryUpsertDto"
                 TId="Guid"
                 Service="@Service"
                 Title="Gestión de Categorías"
                 EntityName="Categoría"
                 Icon="@Icons.Material.Filled.Category"
                 CreateButtonText="Nueva Categoría"
                 EmptyMessage="No hay categorías registradas"
                 TableHeaders="@TableHeaders"
                 TableColumns="@TableColumns"
                 EditorFields="@EditorFields"
                 GetEditorDto="@GetEditorDto"
                 SetEditorFromEntity="@SetEditorFromEntity"
                 GetEntityId="@(e => e.Id)"
                 GetEntityDisplayName="@(e => e.Name)"
                 Validator="@Validator" />
```

## ?? Custom CSS Utilities

```css
.dense-spacing    /* 8px padding */
.compact-spacing  /* 4px padding */
.text-right       /* Right-aligned text */
.text-center      /* Center-aligned text */
```

## ?? Theme Configuration

The theme is automatically applied via `MainLayout.razor`:
```razor
<MudThemeProvider Theme="@HeuristicLogixTheme.Theme" />
```

## ?? Checklist for New Components

- [ ] Use `MaxWidth.ExtraLarge` for containers
- [ ] Set `Elevation="0"` on cards
- [ ] Use `Color.Secondary` for primary action buttons
- [ ] Apply `Dense="true"`, `Hover="true"`, `Striped="true"` to tables
- [ ] Add CSS classes to data columns (`.data-id`, `.data-code`, etc.)
- [ ] Use `HLTextField`, `HLSelect`, `HLAutocomplete` for forms
- [ ] Test in both light mode (dark mode TBD)

## ?? Troubleshooting

**Q: My table isn't dense enough**  
A: Make sure you're using `Dense="true"` and CSS class `hl-table`

**Q: Buttons have default MudBlazor colors**  
A: Check that theme is loaded and use `Color.Secondary` for main actions

**Q: Fonts look wrong**  
A: Verify `app.css` is loaded in `index.html` and Inter/Roboto Mono fonts are imported

---

**Version**: 1.0  
**Last Updated**: 2025  
**Standard**: HL-UI-001
