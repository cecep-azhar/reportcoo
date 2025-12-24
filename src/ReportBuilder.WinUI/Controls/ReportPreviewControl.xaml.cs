using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using ReportBuilder.Core.Models;

namespace ReportBuilder.WinUI.Controls;

/// <summary>
/// A control that displays a preview of a report template.
/// </summary>
public sealed partial class ReportPreviewControl : UserControl
{
    private ReportTemplate? _template;
    private ReportData? _sampleData;

    public ReportPreviewControl()
    {
        this.InitializeComponent();
        CreateSampleData();
    }

    /// <summary>
    /// Sets the template to preview.
    /// </summary>
    public void SetTemplate(ReportTemplate template)
  {
        _template = template;
        UpdatePreview();
    }

    /// <summary>
    /// Sets sample data for preview.
    /// </summary>
    public void SetSampleData(ReportData data)
    {
        _sampleData = data;
 UpdatePreview();
    }

    private void CreateSampleData()
 {
  _sampleData = new ReportData
     {
        Institution = new InstitutionData
   {
   Name = "Rumah Sakit Contoh",
     Address = "Jl. Contoh No. 123, Kota",
     Phone = "(021) 1234567",
       Department = "Departemen Radiologi"
  },
            Patient = new PatientData
            {
      Name = "John Doe",
      MedicalRecordNumber = "RM-001234",
       BirthDate = new DateTime(1990, 5, 15),
     Address = "Jl. Pasien No. 456"
  },
  Examination = new ExaminationData
 {
          Name = "USG Abdomen",
          Date = DateTime.Now,
     Room = "Ruang USG 1",
  ClinicalNotes = "Nyeri perut kanan atas",
 Result = "Kesan: Dalam batas normal"
      },
       Doctor = new DoctorData
   {
  Name = "dr. Jane Smith, Sp.Rad",
            Credentials = "SIP: 123456789"
      },
       CityName = "Jakarta"
     };
    }

    private void UpdatePreview()
    {
if (_template == null) return;

 // Update page size
        UpdatePageSize();

        // Update header
      UpdateHeader();

   // Update content
        UpdateContent();

  // Update footer
        UpdateFooter();
}

  private void UpdatePageSize()
  {
     if (_template == null) return;

        // Calculate size based on paper size (at 72 DPI)
        var (width, height) = _template.PaperSize switch
        {
        PaperSize.A4 => (595, 842),
       PaperSize.A5 => (420, 595),
         PaperSize.Letter => (612, 792),
  PaperSize.FourR => (288, 432), // 4x6 inches
PaperSize.FiveR => (360, 504), // 5x7 inches
      _ => (595, 842)
        };

        if (_template.Orientation == PageOrientation.Landscape)
     {
            (width, height) = (height, width);
     }

   this.Width = width;
        this.Height = height;
  }

    private void UpdateHeader()
    {
        if (_template?.Header == null)
 {
  HeaderBorder.Visibility = Visibility.Collapsed;
     HeaderBorderLine.Visibility = Visibility.Collapsed;
     return;
        }

        HeaderBorder.Visibility = _template.Header.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        HeaderBorderLine.Visibility = _template.Header.ShowBorderBottom ? Visibility.Visible : Visibility.Collapsed;

 if (!_template.Header.IsVisible) return;

        // Left logo
        LeftLogoBorder.Visibility = _template.Header.LeftLogo.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        LeftLogoBorder.Width = _template.Header.LeftLogo.Width;
        LeftLogoBorder.Height = _template.Header.LeftLogo.Height;

        if (_template.Header.LeftLogo.ImageData != null)
        {
     LoadImageToBorder(LeftLogoBorder, _template.Header.LeftLogo.ImageData);
        }
        else if (!string.IsNullOrEmpty(_template.Header.LeftLogo.ImagePath) && System.IO.File.Exists(_template.Header.LeftLogo.ImagePath))
        {
            LoadImageToBorder(LeftLogoBorder, _template.Header.LeftLogo.ImagePath);
     }

        // Right logo
        RightLogoBorder.Visibility = _template.Header.RightLogo.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        RightLogoBorder.Width = _template.Header.RightLogo.Width;
        RightLogoBorder.Height = _template.Header.RightLogo.Height;

        if (_template.Header.RightLogo.ImageData != null)
        {
            LoadImageToBorder(RightLogoBorder, _template.Header.RightLogo.ImageData);
   }
        else if (!string.IsNullOrEmpty(_template.Header.RightLogo.ImagePath) && System.IO.File.Exists(_template.Header.RightLogo.ImagePath))
        {
   LoadImageToBorder(RightLogoBorder, _template.Header.RightLogo.ImagePath);
        }

        // Header lines
        UpdateHeaderLines();
}

    private void UpdateHeaderLines()
    {
        if (_template?.Header == null) return;

        HeaderTextPanel.Children.Clear();

        foreach (var line in _template.Header.Lines.Where(l => l.IsVisible).OrderBy(l => l.Order))
 {
            var text = ResolveText(line.PlaceholderKey ?? line.Text);

            var textBlock = new TextBlock
         {
          Text = text,
            FontSize = line.FontSize,
     FontWeight = line.IsBold ? Microsoft.UI.Text.FontWeights.Bold : Microsoft.UI.Text.FontWeights.Normal,
        FontStyle = line.IsItalic ? Windows.UI.Text.FontStyle.Italic : Windows.UI.Text.FontStyle.Normal,
   TextAlignment = line.Alignment switch
         {
TextAlignment.Left => Microsoft.UI.Xaml.TextAlignment.Left,
              TextAlignment.Right => Microsoft.UI.Xaml.TextAlignment.Right,
        _ => Microsoft.UI.Xaml.TextAlignment.Center
       },
                Foreground = new SolidColorBrush(ParseColor(line.FontColor)),
         Margin = new Thickness(0, line.MarginTop, 0, line.MarginBottom)
          };

  HeaderTextPanel.Children.Add(textBlock);
        }
    }

    private void UpdateContent()
    {
      if (_template == null) return;

        // Update info fields
        UpdateInfoFields();

      // Update image grid
     UpdateImageGrid();

        // Update results
        UpdateResults();
    }

    private void UpdateInfoFields()
  {
        if (_template?.Content.InfoFields == null || !_template.Content.InfoFields.IsVisible)
  {
         InfoFieldsGrid.Visibility = Visibility.Collapsed;
         return;
        }

        InfoFieldsGrid.Visibility = Visibility.Visible;
        // Info fields are statically defined in XAML for the preview
        // In a full implementation, this would be dynamically generated
    }

    private void UpdateImageGrid()
    {
   if (_template?.Content.ImageGrid == null || !_template.Content.ImageGrid.IsVisible)
        {
  ImageGridContainer.Visibility = Visibility.Collapsed;
            return;
        }

        var grid = _template.Content.ImageGrid;
        ImageGridContainer.Visibility = Visibility.Visible;

        // Rebuild grid based on template settings
   ImageGridContainer.RowDefinitions.Clear();
        ImageGridContainer.ColumnDefinitions.Clear();
    ImageGridContainer.Children.Clear();

        for (int i = 0; i < grid.Rows; i++)
        {
    ImageGridContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
   }

  for (int i = 0; i < grid.Columns; i++)
 {
      ImageGridContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        int imageIndex = 1;
        for (int row = 0; row < grid.Rows; row++)
        {
            for (int col = 0; col < grid.Columns; col++)
     {
    if (imageIndex > grid.MaxImages) break;

                var border = new Border
              {
      Margin = new Thickness(grid.ImageSpacing / 2),
       Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 245, 245, 245)),
         CornerRadius = new CornerRadius(grid.CornerRadius)
         };

          if (grid.ShowImageBorder)
        {
        border.BorderBrush = new SolidColorBrush(ParseColor(grid.BorderColor));
        border.BorderThickness = new Thickness(grid.BorderThickness);
      }

           var textBlock = new TextBlock
         {
 Text = $"Gambar {imageIndex}",
           HorizontalAlignment = HorizontalAlignment.Center,
         VerticalAlignment = VerticalAlignment.Center,
         Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray)
    };

                border.Child = textBlock;

                Grid.SetRow(border, row);
          Grid.SetColumn(border, col);
  ImageGridContainer.Children.Add(border);

       imageIndex++;
            }
        }
    }

    private void UpdateResults()
    {
        if (_template?.Content.Results == null || !_template.Content.Results.IsVisible)
    {
      ResultsPanel.Visibility = Visibility.Collapsed;
     return;
 }

      ResultsPanel.Visibility = Visibility.Visible;
    }

    private void UpdateFooter()
    {
   if (_template?.Footer == null || !_template.Footer.IsVisible)
        {
          FooterGrid.Visibility = Visibility.Collapsed;
            return;
        }

        FooterGrid.Visibility = Visibility.Visible;

        // Update date location
        if (_template.Footer.DateLocation.IsVisible)
        {
         var city = _template.Footer.DateLocation.CityName ?? _sampleData?.CityName ?? "Jakarta";
       var date = DateTime.Now.ToString(_template.Footer.DateLocation.DateFormat, 
         new System.Globalization.CultureInfo(_template.Footer.DateLocation.CultureCode));
       DateLocationText.Text = $"{city}, {date}";
   }

        // Update doctor name
        DoctorNameText.Text = _sampleData?.Doctor.Name ?? "[Nama Dokter]";
    }

    private string ResolveText(string? text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        if (_sampleData == null) return text;

        return text switch
        {
   PlaceholderKeys.InstitutionName => _sampleData.Institution.Name,
      PlaceholderKeys.InstitutionAddress => _sampleData.Institution.Address,
     PlaceholderKeys.InstitutionPhone => _sampleData.Institution.Phone,
   PlaceholderKeys.DepartmentName => _sampleData.Institution.Department,
            PlaceholderKeys.PatientName => _sampleData.Patient.Name,
     PlaceholderKeys.PatientMRN => _sampleData.Patient.MedicalRecordNumber,
     PlaceholderKeys.DoctorName => _sampleData.Doctor.Name,
       _ => text
      };
    }

    private void LoadImageToBorder(Border border, byte[] imageData)
    {
 try
        {
     var bitmap = new BitmapImage();
            using var stream = new System.IO.MemoryStream(imageData);
  bitmap.SetSource(stream.AsRandomAccessStream());

      var image = new Image
            {
    Source = bitmap,
Stretch = Stretch.Uniform
    };

            border.Child = image;
            border.Background = null;
        }
        catch
        {
            // Keep placeholder
        }
    }

    private void LoadImageToBorder(Border border, string imagePath)
    {
   try
        {
       var bitmap = new BitmapImage(new Uri(imagePath));
   var image = new Image
            {
             Source = bitmap,
     Stretch = Stretch.Uniform
    };

     border.Child = image;
         border.Background = null;
        }
        catch
        {
       // Keep placeholder
    }
    }

    private static Windows.UI.Color ParseColor(string hexColor)
    {
    if (string.IsNullOrEmpty(hexColor) || !hexColor.StartsWith("#"))
      return Microsoft.UI.Colors.Black;

        try
   {
            hexColor = hexColor.TrimStart('#');
       byte a = 255;
            byte r, g, b;

            if (hexColor.Length == 6)
    {
       r = Convert.ToByte(hexColor.Substring(0, 2), 16);
           g = Convert.ToByte(hexColor.Substring(2, 2), 16);
       b = Convert.ToByte(hexColor.Substring(4, 2), 16);
          }
            else if (hexColor.Length == 8)
  {
                a = Convert.ToByte(hexColor.Substring(0, 2), 16);
 r = Convert.ToByte(hexColor.Substring(2, 2), 16);
    g = Convert.ToByte(hexColor.Substring(4, 2), 16);
      b = Convert.ToByte(hexColor.Substring(6, 2), 16);
            }
    else
        {
         return Microsoft.UI.Colors.Black;
            }

        return Windows.UI.Color.FromArgb(a, r, g, b);
      }
     catch
     {
      return Microsoft.UI.Colors.Black;
        }
    }
}
