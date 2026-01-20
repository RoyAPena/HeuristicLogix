# Fix Import Warnings - HeuristicLogix.Client

## ? Quick Fix Steps

### Step 1: Install Dependencies

Run this PowerShell script from the repository root:

```powershell
.\setup-client-dependencies.ps1
```

Or manually run these commands:

```bash
cd HeuristicLogix.Client

# Install MudBlazor
dotnet add package MudBlazor

# Add reference to Shared project
dotnet add reference ..\HeuristicLogix.Shared\HeuristicLogix.Shared.csproj

# Restore
dotnet restore

cd ..
```

### Step 2: Update PlanningDashboard.razor Imports

The file currently has:
```razor
@using HeuristicLogix.Shared.Interfaces  ? This namespace doesn't exist
@inject MudBlazor.ISnackbar Snackbar      ? Should be just ISnackbar
```

**Fixed imports (already applied)**:
```razor
@page "/planning"
@using System.Diagnostics
@inject ISnackbar Snackbar
@inject IDialogService DialogService
```

### Step 3: Restart Visual Studio

1. Close Visual Studio
2. Reopen the solution
3. Wait for IntelliSense to reload
4. Rebuild solution (Ctrl+Shift+B)

---

## ? What Was Fixed

### 1. **Removed Non-Existent Namespace** ?
- ? Before: `@using HeuristicLogix.Shared.Interfaces`
- ? After: Removed (namespace doesn't exist)

### 2. **Simplified MudBlazor Imports** ?
- ? Before: `@inject MudBlazor.ISnackbar Snackbar`
- ? After: `@inject ISnackbar Snackbar`
- **Reason**: `@using MudBlazor` is now in `_Imports.razor`

### 3. **Updated _Imports.razor** ?
Added global imports:
```razor
@using HeuristicLogix.Client.Shared
@using HeuristicLogix.Client.Services
@using HeuristicLogix.Shared.DTOs
@using HeuristicLogix.Shared.Domain
```

---

## ?? Required NuGet Packages

| Package | Version | Status |
|---------|---------|--------|
| **MudBlazor** | 7.0.0+ | ? To Install |
| **HeuristicLogix.Shared** | Project Ref | ? To Add |

---

## ?? Verification

After running the setup script, verify:

### 1. No Import Warnings
```razor
@inject ISnackbar Snackbar        ? Should be recognized
@inject IDialogService DialogService  ? Should be recognized
```

### 2. Models Recognized
```csharp
private List<Conduce> _conduces = new();     ? Should work
private List<Truck> _trucks = new();         ? Should work
private OverrideReasonTag _selectedReason;   ? Should work
```

### 3. MudBlazor Components Work
```razor
<MudText Typo="Typo.h4">Title</MudText>     ? Should work
<MudDropContainer T="Conduce">...</MudDropContainer>  ? Should work
```

---

## ?? If Warnings Persist

### Option 1: Manual Project File Check

Open `HeuristicLogix.Client/HeuristicLogix.Client.csproj` and verify:

```xml
<Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- Should have this -->
    <PackageReference Include="MudBlazor" Version="7.0.0" />
  </ItemGroup>

  <ItemGroup>
    <!-- Should have this -->
    <ProjectReference Include="..\HeuristicLogix.Shared\HeuristicLogix.Shared.csproj" />
  </ItemGroup>
</Project>
```

### Option 2: Clean and Rebuild

```bash
dotnet clean
dotnet build
```

### Option 3: Delete bin/obj Folders

```powershell
Remove-Item -Recurse -Force HeuristicLogix.Client\bin
Remove-Item -Recurse -Force HeuristicLogix.Client\obj
dotnet restore
dotnet build
```

---

## ?? Summary

**Issue**: Import warnings in PlanningDashboard.razor  
**Cause**: Missing MudBlazor package and Shared project reference  
**Solution**: Run `setup-client-dependencies.ps1` and restart VS  
**Status**: ? Ready to fix

Run the script and the warnings will disappear! ??
