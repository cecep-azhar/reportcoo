namespace ReportBuilder.Core.Models;

/// <summary>
/// Settings for the ReportBuilder library.
/// </summary>
public class ReportBuilderSettings
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
  public string Category { get; set; } = "General";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Default settings keys.
/// </summary>
public static class SettingsKeys
{
    // General
    public const string DefaultTemplateId = "DefaultTemplateId";
    public const string DefaultPaperSize = "DefaultPaperSize";
    public const string DefaultOrientation = "DefaultOrientation";
    
    // Institution (global defaults)
    public const string InstitutionName = "InstitutionName";
    public const string InstitutionAddress = "InstitutionAddress";
    public const string InstitutionPhone = "InstitutionPhone";
    public const string InstitutionEmail = "InstitutionEmail";
    public const string DefaultCityName = "DefaultCityName";
    
    // Paths
    public const string DefaultLogoLeftPath = "DefaultLogoLeftPath";
    public const string DefaultLogoRightPath = "DefaultLogoRightPath";
    public const string DefaultOutputPath = "DefaultOutputPath";
    
    // PDF Settings
    public const string PdfAuthor = "PdfAuthor";
    public const string PdfCreator = "PdfCreator";
    public const string PdfCompression = "PdfCompression";
    
    // Print Settings
    public const string DefaultPrinterName = "DefaultPrinterName";
    public const string PrintCopies = "PrintCopies";
}

/// <summary>
/// Result of a report generation operation.
/// </summary>
public class ReportGenerationResult
{
    public bool Success { get; set; }
    public byte[]? PdfData { get; set; }
    public string? FilePath { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
    public TimeSpan GenerationTime { get; set; }
    public int PageCount { get; set; }

    public static ReportGenerationResult Succeeded(byte[] pdfData, int pageCount, TimeSpan time) => new()
    {
        Success = true,
        PdfData = pdfData,
        PageCount = pageCount,
        GenerationTime = time
    };

    public static ReportGenerationResult Failed(string errorMessage, Exception? exception = null) => new()
    {
        Success = false,
        ErrorMessage = errorMessage,
        Exception = exception
    };
}

/// <summary>
/// Result of a print operation.
/// </summary>
public class PrintResult
{
 public bool Success { get; set; }
    public string? PrinterName { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
    public int CopiesPrinted { get; set; }

    public static PrintResult Succeeded(string printerName, int copies = 1) => new()
    {
        Success = true,
   PrinterName = printerName,
        CopiesPrinted = copies
    };

    public static PrintResult Failed(string errorMessage, Exception? exception = null) => new()
    {
        Success = false,
ErrorMessage = errorMessage,
        Exception = exception
    };
}
