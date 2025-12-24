using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ReportBuilder.Core.Builder;
using ReportBuilder.Core.Models;
using ReportBuilder.Core.Storage;
using Windows.Storage.Pickers;

namespace ReportBuilder.Demo.Pages;

public sealed partial class GeneratePage : Page
{
 private static readonly string DatabasePath = Path.Combine(
  Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
  "ReportBuilderDemo",
        "reportbuilder.db");

 private IReportBuilderUnitOfWork? _unitOfWork;
    private readonly List<string> _imagePaths = new();

    public GeneratePage()
    {
  this.InitializeComponent();
        Loaded += GeneratePage_Loaded;
    }

    private async void GeneratePage_Loaded(object sender, RoutedEventArgs e)
    {
        _unitOfWork = new SqliteUnitOfWork(DatabasePath);
        await _unitOfWork.InitializeDatabaseAsync();
        await LoadTemplatesAsync();
      PatientBirthDatePicker.Date = new DateTimeOffset(1990, 5, 15, 0, 0, 0, TimeSpan.Zero);
    }

 private async Task LoadTemplatesAsync()
    {
        if (_unitOfWork == null) return;

        var templates = await _unitOfWork.Templates.GetAllAsync();
     TemplateComboBox.ItemsSource = templates.Select(t => new ComboBoxItem
    {
            Content = t.Name,
 Tag = t.Id
        }).ToList();

        if (templates.Any())
      {
   TemplateComboBox.SelectedIndex = 0;
        }
}

  private async void AddImagesButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
     picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
     picker.FileTypeFilter.Add(".jpeg");

  var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

  var files = await picker.PickMultipleFilesAsync();
        if (files != null)
        {
    foreach (var file in files)
 {
         _imagePaths.Add(file.Path);
        }
  ImagesListView.ItemsSource = null;
       ImagesListView.ItemsSource = _imagePaths;
   }
    }

    private async void GeneratePdfButton_Click(object sender, RoutedEventArgs e)
    {
        if (_unitOfWork == null) return;

    try
        {
     StatusText.Text = "Generating PDF...";

        var templateId = GetSelectedTemplateId();
       if (templateId == 0)
         {
  StatusText.Text = "Please select a template.";
      return;
   }

   var factory = new ReportBuilderFactory(_unitOfWork);
  var builder = await factory.CreateFromTemplateAsync(templateId);

     ConfigureBuilder(builder);

    // Show save dialog
  var picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
  picker.FileTypeChoices.Add("PDF", new[] { ".pdf" });
  picker.SuggestedFileName = $"Report_{PatientNameBox.Text}_{DateTime.Now:yyyyMMdd}";

          var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
   WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

  var file = await picker.PickSaveFileAsync();
    if (file != null)
     {
   var result = await builder.GeneratePdfAsync(file.Path);

      if (result.Success)
 {
     StatusText.Text = $"PDF generated successfully!\nPath: {file.Path}\nGeneration time: {result.GenerationTime.TotalMilliseconds:F0}ms";
  }
   else
   {
        StatusText.Text = $"Error: {result.ErrorMessage}";
   }
      }
      else
       {
StatusText.Text = "Operation cancelled.";
     }
  }
        catch (Exception ex)
        {
 StatusText.Text = $"Error: {ex.Message}";
        }
    }

    private async void PreviewButton_Click(object sender, RoutedEventArgs e)
    {
        if (_unitOfWork == null) return;

     try
        {
 StatusText.Text = "Generating preview...";

      var templateId = GetSelectedTemplateId();
  if (templateId == 0)
    {
     StatusText.Text = "Please select a template.";
  return;
     }

var factory = new ReportBuilderFactory(_unitOfWork);
 var builder = await factory.CreateFromTemplateAsync(templateId);

     ConfigureBuilder(builder);

      var previewData = await builder.GetPreviewAsync(800);
 if (previewData != null)
  {
      // Save preview to temp file and open
             var tempPath = Path.Combine(Path.GetTempPath(), $"preview_{Guid.NewGuid()}.png");
    await File.WriteAllBytesAsync(tempPath, previewData);

           // Open with default viewer
      var psi = new System.Diagnostics.ProcessStartInfo
              {
 FileName = tempPath,
          UseShellExecute = true
   };
          System.Diagnostics.Process.Start(psi);

                StatusText.Text = "Preview generated!";
            }
else
       {
       StatusText.Text = "Failed to generate preview.";
            }
   }
        catch (Exception ex)
  {
     StatusText.Text = $"Error: {ex.Message}";
   }
    }

    private int GetSelectedTemplateId()
    {
        if (TemplateComboBox.SelectedItem is ComboBoxItem item && item.Tag is int id)
  {
      return id;
        }
        return 0;
    }

    private void ConfigureBuilder(Core.Builder.ReportBuilder builder)
{
    builder
   .WithInstitution(
      InstitutionNameBox.Text,
     InstitutionAddressBox.Text,
      InstitutionPhoneBox.Text)
     .WithPatient(
          PatientNameBox.Text,
     PatientMRNBox.Text,
           PatientBirthDatePicker.Date?.DateTime,
     PatientAddressBox.Text)
 .WithExamination(
          ExamNameBox.Text,
   DateTime.Now,
  ExamRoomBox.Text,
      ClinicalNotesBox.Text,
     ResultBox.Text)
            .WithDoctor(
  DoctorNameBox.Text,
   DoctorCredentialsBox.Text)
 .WithCity(CityBox.Text);

        foreach (var imagePath in _imagePaths)
  {
builder.AddImage(imagePath);
        }
    }
}
