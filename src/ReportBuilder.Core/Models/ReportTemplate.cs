namespace ReportBuilder.Core.Models;

/// <summary>
/// Represents a complete report template with header, content, and footer sections.
/// </summary>
public class ReportTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PaperSize PaperSize { get; set; } = PaperSize.A4;
    public PageOrientation Orientation { get; set; } = PageOrientation.Portrait;
    public PageMargins Margins { get; set; } = new();

    public HeaderSection? Header { get; set; }
    public ContentSection Content { get; set; } = new();
    public FooterSection? Footer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Supported paper sizes for reports.
/// </summary>
public enum PaperSize
{
    A4 = 0,
    A5 = 1,
    Letter = 2,
FourR = 3,  // 4x6 inches photo paper
    FiveR = 4,  // 5x7 inches photo paper
    Custom = 99
}

/// <summary>
/// Page orientation options.
/// </summary>
public enum PageOrientation
{
    Portrait = 0,
    Landscape = 1
}

/// <summary>
/// Page margin settings in millimeters.
/// </summary>
public class PageMargins
{
    public float Top { get; set; } = 20f;
    public float Bottom { get; set; } = 20f;
    public float Left { get; set; } = 20f;
    public float Right { get; set; } = 20f;

    public PageMargins() { }

    public PageMargins(float all)
    {
        Top = Bottom = Left = Right = all;
    }

  public PageMargins(float vertical, float horizontal)
    {
        Top = Bottom = vertical;
        Left = Right = horizontal;
    }

    public PageMargins(float top, float right, float bottom, float left)
  {
        Top = top;
        Right = right;
        Bottom = bottom;
      Left = left;
    }
}

/// <summary>
/// Custom paper size dimensions in millimeters.
/// </summary>
public class CustomPaperSize
{
    public float Width { get; set; }
    public float Height { get; set; }
}
