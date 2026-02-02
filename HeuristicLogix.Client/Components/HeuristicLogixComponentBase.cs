using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HeuristicLogix.Client.Components;

/// <summary>
/// Base component that enforces HeuristicLogix HL-UI-001 standards.
/// All form components should inherit from this to automatically apply
/// Variant.Outlined and Margin.Dense defaults.
/// </summary>
public abstract class HeuristicLogixComponentBase : ComponentBase
{
    /// <summary>
    /// Default variant for all input components: Outlined
    /// </summary>
    protected Variant DefaultVariant => Variant.Outlined;
    
    /// <summary>
    /// Default margin for all input components: Dense
    /// </summary>
    protected Margin DefaultMargin => Margin.Dense;
    
    /// <summary>
    /// Default table density: Dense
    /// </summary>
    protected bool DefaultDense => true;
    
    /// <summary>
    /// Default table hover: Enabled
    /// </summary>
    protected bool DefaultHover => true;
    
    /// <summary>
    /// Default table striped: Enabled
    /// </summary>
    protected bool DefaultStriped => true;
    
    /// <summary>
    /// Default table fixed header: Enabled
    /// </summary>
    protected bool DefaultFixedHeader => true;
    
    /// <summary>
    /// Default card elevation: 0 (using borders instead)
    /// </summary>
    protected int DefaultCardElevation => 0;
    
    /// <summary>
    /// Default container max width: ExtraLarge
    /// </summary>
    protected MaxWidth DefaultMaxWidth => MaxWidth.ExtraLarge;
}
