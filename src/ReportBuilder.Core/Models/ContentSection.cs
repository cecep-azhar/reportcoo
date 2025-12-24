namespace ReportBuilder.Core.Models;

/// <summary>
/// Represents the main content section of a report template.
/// </summary>
public class ContentSection
{
    public int Id { get; set; }
    public int TemplateId { get; set; }

    // Patient/Subject Info Section
    public InfoFieldsLayout InfoFields { get; set; } = new();

    // Image Grid Section
    public ImageGridLayout ImageGrid { get; set; } = new();

    // Results/Notes Section
    public ResultSection Results { get; set; } = new();

    // Additional custom sections
    public List<CustomSection> CustomSections { get; set; } = new();
}

/// <summary>
/// Layout for information fields (patient info, examination info, etc.)
/// </summary>
public class InfoFieldsLayout
{
    public int Id { get; set; }
    public bool IsVisible { get; set; } = true;
    public List<InfoField> Fields { get; set; } = new();
    public int ColumnsCount { get; set; } = 2;  // 1 or 2 column layout
    public float FontSize { get; set; } = 12f;
    public string FontFamily { get; set; } = "Arial";
    public float RowSpacing { get; set; } = 4f;
    public float ColumnSpacing { get; set; } = 20f;
    public bool ShowBorder { get; set; } = false;
    public string BorderColor { get; set; } = "#CCCCCC";
    public float Padding { get; set; } = 8f;
}

/// <summary>
/// A single information field with label and value.
/// </summary>
public class InfoField
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string PlaceholderKey { get; set; } = string.Empty;  // e.g., "{PatientName}"
    public int Column { get; set; } = 0;  // 0 = left column, 1 = right column
    public int Order { get; set; }
  public bool IsVisible { get; set; } = true;
    public bool LabelBold { get; set; } = false;
    public bool ValueBold { get; set; } = false;
    public string Separator { get; set; } = ":";  // Separator between label and value
    public float? LabelWidth { get; set; }  // Fixed label width (null = auto)
}

/// <summary>
/// Image grid layout settings.
/// </summary>
public class ImageGridLayout
{
    public int Id { get; set; }
    public bool IsVisible { get; set; } = true;
    public int Columns { get; set; } = 2;
    public int Rows { get; set; } = 2;
    public int MaxImages { get; set; } = 4;
    public float ImageSpacing { get; set; } = 10f;
    public bool ShowImageBorder { get; set; } = false;
 public string BorderColor { get; set; } = "#CCCCCC";
    public float BorderThickness { get; set; } = 1f;
    public ImageScaleMode ScaleMode { get; set; } = ImageScaleMode.Uniform;
    public float? FixedWidth { get; set; }  // null = auto
    public float? FixedHeight { get; set; }  // null = auto
    public bool ShowImageNumbers { get; set; } = false;
    public string? BackgroundColor { get; set; }
    public float CornerRadius { get; set; } = 0f;
}

/// <summary>
/// Image scaling modes.
/// </summary>
public enum ImageScaleMode
{
    Uniform = 0,        // Maintain aspect ratio, fit within bounds
    Fill = 1,    // Fill entire space, may crop
  Stretch = 2,      // Stretch to fill, may distort
    None = 3            // Original size
}

/// <summary>
/// Results/examination results section.
/// </summary>
public class ResultSection
{
    public int Id { get; set; }
    public bool IsVisible { get; set; } = true;
    public string Label { get; set; } = "Hasil Pemeriksaan";
    public string PlaceholderKey { get; set; } = "{ExamResult}";
    public float FontSize { get; set; } = 12f;
    public string FontFamily { get; set; } = "Arial";
  public bool LabelBold { get; set; } = true;
  public bool ValueBold { get; set; } = false;
public float MinHeight { get; set; } = 50f;
    public bool ShowBorder { get; set; } = false;
    public string BorderColor { get; set; } = "#CCCCCC";
    public float Padding { get; set; } = 8f;
}

/// <summary>
/// Custom section for additional content.
/// </summary>
public class CustomSection
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsVisible { get; set; } = true;
  public CustomSectionType Type { get; set; } = CustomSectionType.Text;
    public string? Content { get; set; }
  public string? PlaceholderKey { get; set; }
    public float FontSize { get; set; } = 12f;
public string FontFamily { get; set; } = "Arial";
 public bool IsBold { get; set; }
    public TextAlignment Alignment { get; set; } = TextAlignment.Left;
    public float MarginTop { get; set; } = 10f;
    public float MarginBottom { get; set; } = 10f;
}

/// <summary>
/// Custom section types.
/// </summary>
public enum CustomSectionType
{
    Text = 0,
    Table = 1,
    Separator = 2,
    Spacer = 3,
    Image = 4
}
