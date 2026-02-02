using MudBlazor;

namespace HeuristicLogix.Client.Theme;

public static class HeuristicLogixTheme
{
    public static MudTheme Theme { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            // BRAND_PRIMARY: Deep Steel Blue
            Primary = "#283593",
            
            // BRAND_SECONDARY: Alert Amber
            Secondary = "#FF8F00",
            
            // APP_BAR_BG: Midnight Navy
            AppbarBackground = "#1A237E",
            AppbarText = "#FFFFFF",
            
            // NAV_DRAWER_BG: Technical Grey
            DrawerBackground = "#F8F9FA",
            DrawerText = "#212529",
            
            // SURFACE_DEFAULT
            Surface = "#FFFFFF",
            
            // BACKGROUND_CANVAS: Subtle contrast for cards
            Background = "#F0F2F5",
            
            // STATUS_SUCCESS
            Success = "#2E7D32",
            
            // STATUS_ERROR
            Error = "#D32F2F",
            
            // Additional semantic colors
            Warning = "#FF8F00",
            Info = "#283593",
            
            // Text colors
            TextPrimary = "#212529",
            TextSecondary = "#6C757D",
            TextDisabled = "#ADB5BD",
            
            // Action colors
            ActionDefault = "#283593",
            ActionDisabled = "#E0E0E0",
            
            // Dividers and borders
            Divider = "#E0E0E0",
            DividerLight = "#F0F0F0",
            
            // Table specific
            TableLines = "#E0E0E0",
            TableStriped = "#F8F9FA",
            TableHover = "#F0F2F5"
        },
        
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "4px",
            DrawerWidthLeft = "240px",
            DrawerWidthRight = "240px",
            AppbarHeight = "64px"
        }
    };
}


