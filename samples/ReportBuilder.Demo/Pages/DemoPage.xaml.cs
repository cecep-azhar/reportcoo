using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ReportBuilder.Core.Builder;
using ReportBuilder.Core.Models;
using ReportBuilder.Core.Services;

namespace ReportBuilder.Demo.Pages;

public sealed partial class DemoPage : Page
{
 private static readonly string OutputFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
"ReportBuilder Demo Output");

    public DemoPage()
 {
    this.InitializeComponent();
    }

    private async void QuickGenerateButton_Click(object sender, RoutedEventArgs e)
    {
        LoadingRing.IsActive = true;
   QuickGenerateButton.IsEnabled = false;
        StatusText.Text = "Generating PDF...";

        try
        {
      // Create output folder if not exists
      if (!Directory.Exists(OutputFolder))
  {
          Directory.CreateDirectory(OutputFolder);
  }

       // Build a template programmatically
   var template = new TemplateBuilder()
        .WithName("Demo Template")
         .WithPaperSize(PaperSize.A4)
         .WithOrientation(PageOrientation.Portrait)
  .WithMargins(20)
     .WithHeader(h => h
           .WithHeight(100)
     .WithLeftLogo(70, 70)
    .WithRightLogo(70, 70)
       .AddLine("RUMAH SAKIT DEMO", 18, true)
    .AddLine("Departemen Radiologi", 14, true)
        .AddLine("Jl. Demo No. 123, Jakarta 12345", 10)
       .AddLine("Telp: (021) 1234567 | Email: info@rsdemo.com", 9)
          .WithBorderBottom())
        .WithContent(c => c
    .WithInfoFields(f => f
          .WithColumns(2)
         .AddField("Nama Pasien", PlaceholderKeys.PatientName, 0)
.AddField("Jenis Pemeriksaan", PlaceholderKeys.ExamName, 1)
    .AddField("Tanggal Lahir", PlaceholderKeys.PatientBirthDate, 0)
         .AddField("Keterangan Klinis", PlaceholderKeys.ClinicalNotes, 1)
        .AddField("No. RM", PlaceholderKeys.PatientMRN, 0)
          .AddField("Alamat", PlaceholderKeys.PatientAddress, 1))
    .WithImageGrid(2, 2, 4, 10)
        .WithResults("Hasil Pemeriksaan", true))
 .WithFooter(f => f
   .WithDateLocation("Jakarta", "dd MMMM yyyy")
   .WithSignature("Dokter Pemeriksa", 60))
 .Build();

     // Create report with sample data
      var generator = new PdfReportGenerator();
         var builder = new Core.Builder.ReportBuilder(template, generator);

       var outputPath = Path.Combine(OutputFolder, $"DemoReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

 var result = await builder
 .WithInstitution(
 "RS Demo",
    "Jl. Demo No. 123, Jakarta",
 "(021) 1234567",
      "info@rsdemo.com",
         "Radiologi")
      .WithPatient(
    "Budi Santoso",
            "RM-2024-001234",
    new DateTime(1985, 7, 20),
   "Jl. Pasien No. 456, Jakarta Selatan",
        "Laki-laki")
         .WithExamination(
     "USG Abdomen",
       DateTime.Now,
  "Ruang USG 1",
         "Nyeri perut kanan atas sejak 3 hari yang lalu",
        "Hepar : Ukuran dalam batas normal, echostruktur homogen, tidak tampak nodul/massa.\n\n" +
      "Vesica Felea : Ukuran normal, dinding tidak menebal, tidak tampak batu.\n\n" +
        "Pancreas : Ukuran normal, echostruktur homogen.\n\n" +
   "Lien : Ukuran normal, echostruktur homogen.\n\n" +
  "Ren Kanan : Ukuran normal, tidak tampak batu/hidronefrosis.\n\n" +
  "Ren Kiri : Ukuran normal, tidak tampak batu/hidronefrosis.\n\n" +
   "VU : Terisi cukup, dinding tidak menebal.\n\n" +
             "KESAN: USG Abdomen dalam batas normal.")
.WithDoctor("dr. Dewi Anggraini, Sp.Rad", "SIP: 449/2020/DKI")
       .WithCity("Jakarta")
                .GeneratePdfAsync(outputPath);

  if (result.Success)
   {
      StatusText.Text = $"? PDF berhasil dibuat!\n\n" +
       $"?? File: {Path.GetFileName(outputPath)}\n" +
     $"?? Lokasi: {OutputFolder}\n" +
     $"?? Waktu: {result.GenerationTime.TotalMilliseconds:F0}ms\n" +
               $"?? Ukuran: {result.PdfData?.Length / 1024.0:F1} KB";

      // Open the PDF
   var psi = new System.Diagnostics.ProcessStartInfo
        {
     FileName = outputPath,
        UseShellExecute = true
          };
    System.Diagnostics.Process.Start(psi);
   }
   else
   {
       StatusText.Text = $"? Gagal: {result.ErrorMessage}";
  }
        }
   catch (Exception ex)
     {
     StatusText.Text = $"? Error: {ex.Message}";
        }
        finally
     {
    LoadingRing.IsActive = false;
   QuickGenerateButton.IsEnabled = true;
     }
    }

    private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
    {
  if (!Directory.Exists(OutputFolder))
     {
 Directory.CreateDirectory(OutputFolder);
        }

     var psi = new System.Diagnostics.ProcessStartInfo
      {
  FileName = OutputFolder,
            UseShellExecute = true
    };
      System.Diagnostics.Process.Start(psi);
    }
}
