using System.Diagnostics;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ReportBuilder.Core.Models;

namespace ReportBuilder.Core.Services;

/// <summary>
/// PDF report generator using QuestPDF.
/// </summary>
public class PdfReportGenerator : IReportGenerator
{
    private readonly IPlaceholderResolver _placeholderResolver;

    static PdfReportGenerator()
    {
  // Set QuestPDF license (Community license for open source/non-commercial)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public PdfReportGenerator()
    {
        _placeholderResolver = new PlaceholderResolver();
    }

    public PdfReportGenerator(IPlaceholderResolver placeholderResolver)
    {
   _placeholderResolver = placeholderResolver;
    }

    public async Task<ReportGenerationResult> GeneratePdfAsync(ReportTemplate template, ReportData data)
    {
 var stopwatch = Stopwatch.StartNew();

        try
  {
            var document = CreateDocument(template, data);
            var pdfBytes = document.GeneratePdf();
  stopwatch.Stop();

            // Count pages (approximate - QuestPDF doesn't expose this directly)
            var pageCount = EstimatePageCount(pdfBytes);

    return await Task.FromResult(ReportGenerationResult.Succeeded(pdfBytes, pageCount, stopwatch.Elapsed));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
      return await Task.FromResult(ReportGenerationResult.Failed(ex.Message, ex));
        }
}

    public async Task<ReportGenerationResult> GeneratePdfToFileAsync(ReportTemplate template, ReportData data, string outputPath)
    {
        var result = await GeneratePdfAsync(template, data);

    if (result.Success && result.PdfData != null)
 {
   var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
    {
       Directory.CreateDirectory(directory);
  }

  await File.WriteAllBytesAsync(outputPath, result.PdfData);
          result.FilePath = outputPath;
        }

        return result;
    }

    public Task<PrintResult> PrintAsync(ReportTemplate template, ReportData data, string printerName, int copies = 1)
    {
        // Note: Direct printing from QuestPDF requires platform-specific implementation
        // This is a placeholder - actual printing should be implemented in the WinUI layer
        return Task.FromResult(PrintResult.Failed("Direct printing not implemented. Use GeneratePdfAsync and print the PDF file."));
    }

    public async Task<byte[]?> GetPreviewImageAsync(ReportTemplate template, ReportData data, int width = 800)
    {
        try
        {
  var document = CreateDocument(template, data);
  var images = document.GenerateImages(new ImageGenerationSettings
     {
        ImageFormat = ImageFormat.Png,
  RasterDpi = 150
         });

            // Return first page only
    var firstPage = images.FirstOrDefault();
            return await Task.FromResult(firstPage);
        }
  catch
  {
            return null;
        }
  }

    private IDocument CreateDocument(ReportTemplate template, ReportData data)
    {
        return Document.Create(container =>
        {
    container.Page(page =>
            {
     // Page settings
              ConfigurePage(page, template);

         // Header
         if (template.Header?.IsVisible == true)
     {
        page.Header().Element(c => ComposeHeader(c, template.Header, data));
 }

     // Content
      page.Content().Element(c => ComposeContent(c, template, data));

      // Footer
    if (template.Footer?.IsVisible == true)
       {
             page.Footer().Element(c => ComposeFooter(c, template.Footer, data));
       }
});
        });
    }

    private void ConfigurePage(PageDescriptor page, ReportTemplate template)
    {
        // Paper size
        var pageSize = template.PaperSize switch
        {
   PaperSize.A4 => PageSizes.A4,
      PaperSize.A5 => PageSizes.A5,
            PaperSize.Letter => PageSizes.Letter,
   PaperSize.FourR => new PageSize(4f * 72, 6f * 72), // 4x6 inches
       PaperSize.FiveR => new PageSize(5f * 72, 7f * 72), // 5x7 inches
          _ => PageSizes.A4
        };

        // Orientation
    if (template.Orientation == PageOrientation.Landscape)
        {
    pageSize = new PageSize(pageSize.Height, pageSize.Width);
        }

      page.Size(pageSize);

  // Margins
        page.MarginTop(template.Margins.Top, Unit.Millimetre);
        page.MarginBottom(template.Margins.Bottom, Unit.Millimetre);
        page.MarginLeft(template.Margins.Left, Unit.Millimetre);
        page.MarginRight(template.Margins.Right, Unit.Millimetre);

        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));
    }

    private void ComposeHeader(IContainer container, HeaderSection header, ReportData data)
    {
        container.Column(column =>
   {
     column.Item().Row(row =>
   {
                // Left logo
       if (header.LeftLogo.IsVisible)
                {
          row.ConstantItem(header.LeftLogo.Width, Unit.Millimetre).AlignMiddle().Element(c =>
       {
            var logoData = header.LeftLogo.ImageData ?? data.Institution.LogoLeft;
     if (logoData != null)
         {
     c.Image(logoData).FitArea();
         }
                  else if (!string.IsNullOrEmpty(header.LeftLogo.ImagePath) && File.Exists(header.LeftLogo.ImagePath))
   {
         c.Image(header.LeftLogo.ImagePath).FitArea();
         }
         });
          }

 // Center text (header lines)
         row.RelativeItem().AlignMiddle().Column(textColumn =>
          {
     foreach (var line in header.Lines.Where(l => l.IsVisible).OrderBy(l => l.Order))
                {
         var text = !string.IsNullOrEmpty(line.PlaceholderKey)
    ? _placeholderResolver.Resolve(line.PlaceholderKey, data) ?? line.Text
          : line.Text;

                if (string.IsNullOrEmpty(text)) continue;

     textColumn.Item().PaddingVertical(line.MarginTop, Unit.Millimetre).AlignCenter().Text(text).Style(style =>
  {
           style = style.FontSize(line.FontSize).FontColor(line.FontColor);
   if (line.IsBold) style = style.Bold();
             if (line.IsItalic) style = style.Italic();
  if (line.IsUnderline) style = style.Underline();
         return style;
            });
        }
  });

        // Right logo
       if (header.RightLogo.IsVisible)
    {
        row.ConstantItem(header.RightLogo.Width, Unit.Millimetre).AlignMiddle().Element(c =>
 {
   var logoData = header.RightLogo.ImageData ?? data.Institution.LogoRight;
    if (logoData != null)
   {
      c.Image(logoData).FitArea();
  }
    else if (!string.IsNullOrEmpty(header.RightLogo.ImagePath) && File.Exists(header.RightLogo.ImagePath))
     {
      c.Image(header.RightLogo.ImagePath).FitArea();
        }
});
    }
});

 // Border bottom
            if (header.ShowBorderBottom)
   {
       column.Item().PaddingTop(5).LineHorizontal(header.BorderThickness).LineColor(header.BorderColor);
         }
        });
    }

    private void ComposeContent(IContainer container, ReportTemplate template, ReportData data)
    {
   container.PaddingVertical(10).Column(column =>
 {
            var content = template.Content;

 // Info fields section
    if (content.InfoFields.IsVisible && content.InfoFields.Fields.Any())
            {
     column.Item().Element(c => ComposeInfoFields(c, content.InfoFields, data));
                column.Item().PaddingVertical(10);
            }

            // Image grid section
            if (content.ImageGrid.IsVisible && data.Images.Any())
            {
     column.Item().Element(c => ComposeImageGrid(c, content.ImageGrid, data));
  column.Item().PaddingVertical(10);
       }

        // Result section
 if (content.Results.IsVisible)
            {
    column.Item().Element(c => ComposeResultSection(c, content.Results, data));
            }

      // Custom sections
         foreach (var customSection in content.CustomSections.Where(s => s.IsVisible).OrderBy(s => s.Order))
            {
 column.Item().PaddingTop(customSection.MarginTop).PaddingBottom(customSection.MarginBottom)
           .Element(c => ComposeCustomSection(c, customSection, data));
}
   });
}

    private void ComposeInfoFields(IContainer container, InfoFieldsLayout layout, ReportData data)
    {
      var visibleFields = layout.Fields.Where(f => f.IsVisible).OrderBy(f => f.Order).ToList();
        if (!visibleFields.Any()) return;

        if (layout.ShowBorder)
        {
            container = container.Border((float)layout.Padding / 4).BorderColor(layout.BorderColor);
      }

        container.Padding(layout.Padding).Table(table =>
        {
  // Define columns based on layout
            if (layout.ColumnsCount == 2)
  {
      table.ColumnsDefinition(columns =>
      {
   columns.RelativeColumn(); // Label 1
         columns.ConstantColumn(10); // Separator 1
          columns.RelativeColumn(); // Value 1
         columns.ConstantColumn(20); // Gap
 columns.RelativeColumn(); // Label 2
 columns.ConstantColumn(10); // Separator 2
          columns.RelativeColumn(); // Value 2
    });

                // Group fields by order (row)
          var rows = visibleFields.GroupBy(f => f.Order).OrderBy(g => g.Key);

     foreach (var row in rows)
       {
     var leftField = row.FirstOrDefault(f => f.Column == 0);
var rightField = row.FirstOrDefault(f => f.Column == 1);

 // Left column
         if (leftField != null)
            {
      table.Cell().Text(leftField.Label).Style(s => leftField.LabelBold ? s.Bold() : s);
      table.Cell().Text(leftField.Separator);
           var leftValue = _placeholderResolver.Resolve(leftField.PlaceholderKey, data) ?? "";
         table.Cell().Text(leftValue).Style(s => leftField.ValueBold ? s.Bold() : s);
 }
         else
       {
      table.Cell();
       table.Cell();
               table.Cell();
    }

        table.Cell(); // Gap

    // Right column
        if (rightField != null)
         {
        table.Cell().Text(rightField.Label).Style(s => rightField.LabelBold ? s.Bold() : s);
          table.Cell().Text(rightField.Separator);
 var rightValue = _placeholderResolver.Resolve(rightField.PlaceholderKey, data) ?? "";
   table.Cell().Text(rightValue).Style(s => rightField.ValueBold ? s.Bold() : s);
               }
                    else
      {
      table.Cell();
            table.Cell();
table.Cell();
            }
           }
            }
            else // Single column
        {
     table.ColumnsDefinition(columns =>
           {
   columns.RelativeColumn(1); // Label
          columns.ConstantColumn(10); // Separator
         columns.RelativeColumn(2); // Value
                });

       foreach (var field in visibleFields)
    {
 table.Cell().Text(field.Label).Style(s => field.LabelBold ? s.Bold() : s);
        table.Cell().Text(field.Separator);
 var value = _placeholderResolver.Resolve(field.PlaceholderKey, data) ?? "";
         table.Cell().Text(value).Style(s => field.ValueBold ? s.Bold() : s);
     }
        }
        });
  }

    private void ComposeImageGrid(IContainer container, ImageGridLayout grid, ReportData data)
    {
     var images = data.Images.OrderBy(i => i.Order).Take(grid.MaxImages).ToList();
        if (!images.Any()) return;

   container.Table(table =>
        {
   table.ColumnsDefinition(columns =>
            {
                for (int i = 0; i < grid.Columns; i++)
   {
         columns.RelativeColumn();
     }
            });

     var imageIndex = 0;
        for (int row = 0; row < grid.Rows && imageIndex < images.Count; row++)
       {
    for (int col = 0; col < grid.Columns && imageIndex < images.Count; col++)
          {
    var image = images[imageIndex];
       table.Cell().Padding(grid.ImageSpacing / 2).Element(cell =>
 {
          var imageContainer = cell;

           if (grid.ShowImageBorder)
{
      imageContainer = imageContainer.Border(grid.BorderThickness).BorderColor(grid.BorderColor);
                }

            if (grid.CornerRadius > 0)
          {
       // QuestPDF doesn't support corner radius directly, skip for now
     }

     imageContainer.Element(imgCell =>
      {
      byte[]? imageData = image.Data;
         if (imageData == null && !string.IsNullOrEmpty(image.FilePath) && File.Exists(image.FilePath))
  {
          imageData = File.ReadAllBytes(image.FilePath);
  }

            if (imageData != null)
       {
         var img = imgCell.Image(imageData);
          switch (grid.ScaleMode)
            {
           case ImageScaleMode.Uniform:
                  img.FitArea();
          break;
        case ImageScaleMode.Fill:
   img.FitWidth();
        break;
       case ImageScaleMode.Stretch:
     img.FitArea();
       break;
       default:
 img.FitArea();
           break;
                }
        }
   });
               });

       imageIndex++;
                }
            }
     });
    }

    private void ComposeResultSection(IContainer container, ResultSection result, ReportData data)
 {
        var resultText = _placeholderResolver.Resolve(result.PlaceholderKey, data) ?? "";

 if (result.ShowBorder)
        {
         container = container.Border(1).BorderColor(result.BorderColor);
   }

        container.Padding(result.Padding).MinHeight(result.MinHeight).Column(column =>
        {
     // Label
            column.Item().Text(result.Label + " :").Style(style =>
            {
          style = style.FontSize(result.FontSize);
       if (result.LabelBold) style = style.Bold();
        return style;
      });

       // Value
            column.Item().PaddingTop(5).Text(string.IsNullOrEmpty(resultText) ? "-" : resultText).Style(style =>
       {
                style = style.FontSize(result.FontSize);
           if (result.ValueBold) style = style.Bold();
            return style;
       });
        });
    }

    private void ComposeCustomSection(IContainer container, CustomSection section, ReportData data)
  {
        var content = !string.IsNullOrEmpty(section.PlaceholderKey)
        ? _placeholderResolver.Resolve(section.PlaceholderKey, data)
            : section.Content;

        if (string.IsNullOrEmpty(content)) return;

        switch (section.Type)
  {
          case CustomSectionType.Text:
    var textContainer = container.Text(content);
        textContainer.Style(style =>
     {
                 style = style.FontSize(section.FontSize);
         if (section.IsBold) style = style.Bold();
          return style;
                });
           break;

            case CustomSectionType.Separator:
     container.LineHorizontal(1).LineColor("#CCCCCC");
  break;

 case CustomSectionType.Spacer:
      container.Height(float.Parse(content ?? "20"));
        break;
        }
    }

    private void ComposeFooter(IContainer container, FooterSection footer, ReportData data)
    {
        container.Column(column =>
        {
     // Border top
   if (footer.ShowBorderTop)
            {
             column.Item().PaddingBottom(5).LineHorizontal(footer.BorderThickness).LineColor(footer.BorderColor);
            }

   column.Item().Row(row =>
          {
    // Result section (left side, if needed)
     row.RelativeItem(2);

              // Empty middle
        row.RelativeItem(1);

        // Signature section (right side)
      row.RelativeItem(1).Column(sigColumn =>
                {
   // Date and location
          if (footer.DateLocation.IsVisible)
     {
           var dateText = !string.IsNullOrEmpty(footer.DateLocation.CustomText)
           ? footer.DateLocation.CustomText
       : FormatDateLocation(footer.DateLocation, data);

        sigColumn.Item().AlignCenter().Text(dateText).FontSize(footer.DateLocation.FontSize);
  }

   // Signature title
               if (footer.Signature.IsVisible)
     {
     sigColumn.Item().AlignCenter().Text(footer.Signature.TitleLabel)
 .FontSize(footer.Signature.FontSize);

            // Signature space
         sigColumn.Item().Height(footer.Signature.SignatureSpaceHeight);

              // Signature line
             if (footer.Signature.ShowSignatureLine)
   {
          sigColumn.Item().AlignCenter().Width(footer.Signature.SignatureLineWidth)
        .LineHorizontal(1).LineColor("#000000");
 }

         // Doctor name
        var doctorName = _placeholderResolver.Resolve(footer.Signature.NamePlaceholder, data) ?? "-";
             sigColumn.Item().AlignCenter().Text(doctorName).FontSize(footer.Signature.FontSize);

            // Credentials
            if (!string.IsNullOrEmpty(footer.Signature.CredentialsPlaceholder))
       {
   var credentials = _placeholderResolver.Resolve(footer.Signature.CredentialsPlaceholder, data);
     if (!string.IsNullOrEmpty(credentials))
       {
             sigColumn.Item().AlignCenter().Text(credentials).FontSize(footer.Signature.FontSize - 1);
           }
           }
         }
     });
    });

         // Additional footer elements
   foreach (var element in footer.AdditionalElements.Where(e => e.IsVisible).OrderBy(e => e.Order))
            {
  column.Item().Element(c => ComposeFooterElement(c, element, data));
            }
        });
    }

    private void ComposeFooterElement(IContainer container, FooterElement element, ReportData data)
    {
        var content = !string.IsNullOrEmpty(element.PlaceholderKey)
            ? _placeholderResolver.Resolve(element.PlaceholderKey, data)
            : element.Content;

    if (string.IsNullOrEmpty(content) && element.Type != FooterElementType.Separator) return;

        IContainer alignedContainer = element.Position switch
        {
    HorizontalPosition.Left => container.AlignLeft(),
            HorizontalPosition.Center => container.AlignCenter(),
            HorizontalPosition.Right => container.AlignRight(),
    _ => container.AlignCenter()
    };

      switch (element.Type)
        {
            case FooterElementType.Text:
                alignedContainer.Text(content ?? "").Style(style =>
      {
  style = style.FontSize(element.FontSize).FontColor(element.FontColor);
  if (element.IsBold) style = style.Bold();
        if (element.IsItalic) style = style.Italic();
    return style;
    });
        break;

    case FooterElementType.PageNumber:
     alignedContainer.Text(text =>
        {
   text.Span("Halaman ").FontSize(element.FontSize);
        text.CurrentPageNumber().FontSize(element.FontSize);
      text.Span(" dari ").FontSize(element.FontSize);
      text.TotalPages().FontSize(element.FontSize);
      });
        break;

            case FooterElementType.DateTime:
      alignedContainer.Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(element.FontSize);
       break;

  case FooterElementType.Separator:
                container.LineHorizontal(1).LineColor("#CCCCCC");
 break;
   }
    }

    private string FormatDateLocation(DateLocationBlock dateLocation, ReportData data)
    {
        var culture = new CultureInfo(dateLocation.CultureCode);
        var date = data.GeneratedAt.ToString(dateLocation.DateFormat, culture);

        if (!string.IsNullOrEmpty(dateLocation.CityName) || !string.IsNullOrEmpty(data.CityName))
        {
  var city = dateLocation.CityName ?? data.CityName;
            return $"{city}, {date}";
 }

        return date;
    }

  private static int EstimatePageCount(byte[] pdfBytes)
    {
        // Simple estimation based on PDF structure
        // A more accurate count would require parsing the PDF
        var content = System.Text.Encoding.ASCII.GetString(pdfBytes);
        return System.Text.RegularExpressions.Regex.Matches(content, @"/Type\s*/Page[^s]").Count;
    }
}
