using ReportBuilder.Core.Models;

namespace ReportBuilder.Core.Services;

/// <summary>
/// Interface for report generation services.
/// </summary>
public interface IReportGenerator
{
    /// <summary>
  /// Generates a PDF report from a template and data.
    /// </summary>
   Task<ReportGenerationResult> GeneratePdfAsync(ReportTemplate template, ReportData data);

    /// <summary>
    /// Generates a PDF report and saves it to a file.
    /// </summary>
   Task<ReportGenerationResult> GeneratePdfToFileAsync(ReportTemplate template, ReportData data, string outputPath);

    /// <summary>
    /// Prints a report directly to a printer.
    /// </summary>
    Task<PrintResult> PrintAsync(ReportTemplate template, ReportData data, string printerName, int copies = 1);

    /// <summary>
    /// Gets preview image bytes for a report (first page).
    /// </summary>
    Task<byte[]?> GetPreviewImageAsync(ReportTemplate template, ReportData data, int width = 800);
}

/// <summary>
/// Interface for placeholder resolution.
/// </summary>
public interface IPlaceholderResolver
{
    /// <summary>
    /// Resolves a placeholder key to its value.
    /// </summary>
    string? Resolve(string placeholderKey, ReportData data);

    /// <summary>
    /// Resolves all placeholders in a text string.
    /// </summary>
    string ResolveText(string text, ReportData data);
}
