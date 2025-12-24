namespace ReportBuilder.Core.Models;

/// <summary>
/// Represents the footer section of a report template.
/// </summary>
public class FooterSection
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public bool IsVisible { get; set; } = true;
  public float Height { get; set; } = 120f;

    // Signature Section
    public SignatureBlock Signature { get; set; } = new();

    // Date/Location Block
    public DateLocationBlock DateLocation { get; set; } = new();

    // Additional Footer Elements
    public List<FooterElement> AdditionalElements { get; set; } = new();

    // Border Settings
    public bool ShowBorderTop { get; set; } = false;
    public string BorderColor { get; set; } = "#000000";
    public float BorderThickness { get; set; } = 1f;

    // Background
    public string? BackgroundColor { get; set; }
}

/// <summary>
/// Signature block settings.
/// </summary>
public class SignatureBlock
{
    public int Id { get; set; }
    public bool IsVisible { get; set; } = true;
    public string TitleLabel { get; set; } = "Dokter Pemeriksa";
    public string NamePlaceholder { get; set; } = "{DoctorName}";
    public string? CredentialsPlaceholder { get; set; } = "{DoctorCredentials}";  // e.g., SIP number
    public float SignatureSpaceHeight { get; set; } = 60f;
  public HorizontalPosition Position { get; set; } = HorizontalPosition.Right;
    public float FontSize { get; set; } = 12f;
    public string FontFamily { get; set; } = "Arial";
    public bool ShowSignatureLine { get; set; } = false;
    public float SignatureLineWidth { get; set; } = 150f;
}

/// <summary>
/// Date and location block settings.
/// </summary>
public class DateLocationBlock
{
    public int Id { get; set; }
    public bool IsVisible { get; set; } = true;
    public string CityName { get; set; } = string.Empty;
    public string DateFormat { get; set; } = "dd MMMM yyyy";
    public string CultureCode { get; set; } = "id-ID";  // Indonesian culture for date formatting
    public HorizontalPosition Position { get; set; } = HorizontalPosition.Right;
    public float FontSize { get; set; } = 12f;
    public string FontFamily { get; set; } = "Arial";
    public string? CustomText { get; set; }  // Override with custom text if set
}

/// <summary>
/// Horizontal position options.
/// </summary>
public enum HorizontalPosition
{
    Left = 0,
    Center = 1,
    Right = 2
}

/// <summary>
/// Additional footer element.
/// </summary>
public class FooterElement
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public FooterElementType Type { get; set; } = FooterElementType.Text;
    public string? Content { get; set; }
    public string? PlaceholderKey { get; set; }
    public HorizontalPosition Position { get; set; } = HorizontalPosition.Center;
    public float FontSize { get; set; } = 10f;
    public string FontFamily { get; set; } = "Arial";
    public string FontColor { get; set; } = "#000000";
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    public bool IsVisible { get; set; } = true;
}

/// <summary>
/// Footer element types.
/// </summary>
public enum FooterElementType
{
    Text = 0,
    PageNumber = 1,
DateTime = 2,
  Image = 3,
    Separator = 4
}
