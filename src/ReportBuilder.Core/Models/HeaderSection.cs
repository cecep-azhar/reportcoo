namespace ReportBuilder.Core.Models;

/// <summary>
/// Represents the header section of a report template.
/// </summary>
public class HeaderSection
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public bool IsVisible { get; set; } = true;
    public float Height { get; set; } = 100f;

    // Logo Settings
    public LogoPlacement LeftLogo { get; set; } = new();
    public LogoPlacement RightLogo { get; set; } = new();

    // Header Lines (customizable text lines)
    public List<HeaderLine> Lines { get; set; } = new();

    // Border Settings
    public bool ShowBorderBottom { get; set; } = true;
    public string BorderColor { get; set; } = "#000000";
    public float BorderThickness { get; set; } = 1f;

    // Background
    public string? BackgroundColor { get; set; }
}

/// <summary>
/// Logo placement and settings.
/// </summary>
public class LogoPlacement
{
    public bool IsVisible { get; set; } = true;
    public string? ImagePath { get; set; }
    public byte[]? ImageData { get; set; }  // For embedded logos stored in database
    public float Width { get; set; } = 80f;
    public float Height { get; set; } = 80f;
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Center;
}

/// <summary>
/// A single line of text in the header.
/// </summary>
public class HeaderLine
{
    public int Id { get; set; }
    public int HeaderSectionId { get; set; }
    public int Order { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? PlaceholderKey { get; set; }  // e.g., "{InstitutionName}"
    public float FontSize { get; set; } = 12f;
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    public bool IsUnderline { get; set; }
    public string FontFamily { get; set; } = "Arial";
    public string FontColor { get; set; } = "#000000";
    public TextAlignment Alignment { get; set; } = TextAlignment.Center;
    public bool IsVisible { get; set; } = true;
    public float MarginTop { get; set; } = 2f;
    public float MarginBottom { get; set; } = 2f;
}

/// <summary>
/// Vertical alignment options.
/// </summary>
public enum VerticalAlignment
{
    Top = 0,
    Center = 1,
    Bottom = 2
}

/// <summary>
/// Text alignment options.
/// </summary>
public enum TextAlignment
{
    Left = 0,
    Center = 1,
 Right = 2,
    Justify = 3
}
