# SPECKIT: HEURISTICLOGIX VISUAL DNA & LAYOUT
# Version: 1.1 (Updated: 2026-02-02)

[CORE_PHILOSOPHY]
- Strategy: Data-Driven Density (DDD).
- Objective: Minimize scroll, maximize information scannability.
- Aesthetic: Industrial Modern (Precision over Ornament).

[COLOR_PALETTE_HEX]
- BRAND_PRIMARY: #283593 (Deep Steel Blue)
- BRAND_SECONDARY: #FF8F00 (Alert Amber)
- APP_BAR_BG: #1A237E (Midnight Navy)
- NAV_DRAWER_BG: #F8F9FA (Technical Grey)
- SURFACE_DEFAULT: #FFFFFF
- BACKGROUND_CANVAS: #F0F2F5
- STATUS_SUCCESS: #2E7D32
- STATUS_ERROR: #D32F2F

[COMPONENT_SPECIFICATIONS]
- [cite_start]FORM_FACTOR: Variant="Variant.Outlined", Margin="Margin.Dense" [cite: 14, 15]
- [cite_start]TABLES: Dense="true", Hover="true", Striped="true", FixedHeader="true", Elevation="0" [cite: 6, 7]
- [cite_start]BUTTONS: Primary Actions use BRAND_SECONDARY (Amber), Secondary use BRAND_PRIMARY [cite: 3, 10, 12]
- TYPOGRAPHY: Primary: 'Inter'. Secondary: 'Roboto Mono' (for SKUs and Prices).

[LAYOUT_HIERARCHY]
- SIDEBAR: Persistent (Non-overlay), ClipMode.Always.
- HEADER: Sticky, Elevation 1.
- [cite_start]CONTAINER: MaxWidth="MaxWidth.ExtraLarge" [cite: 1]

[GLOBAL_DEPENDENCIES]
- [cite_start]DIALOG_PROVIDER: Must exist as <MudDialogProvider /> in MainLayout.razor (Required for DialogVisible) [cite: 25, 33]
- SNACKBAR_PROVIDER: Must exist as <MudSnackbarProvider />.
- NOTIFICATIONS: Snackbar position: TopRight. Duration: 5s.