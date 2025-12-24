using ReportBuilder.Core.Builder;
using ReportBuilder.Core.Models;
using ReportBuilder.Core.Services;

namespace ReportBuilder.Tests;

public class ReportBuilderTests
{
    [Fact]
    public void TemplateBuilder_ShouldCreateValidTemplate()
  {
        // Arrange & Act
        var template = new TemplateBuilder()
         .WithName("Test Template")
        .WithDescription("Test description")
          .WithPaperSize(PaperSize.A4)
       .WithOrientation(PageOrientation.Portrait)
      .WithMargins(20)
   .WithHeader(h => h
      .WithHeight(100)
       .WithLeftLogo(80, 80)
     .WithRightLogo(80, 80)
       .AddPlaceholderLine(PlaceholderKeys.InstitutionName, 16, true)
        .AddPlaceholderLine(PlaceholderKeys.DepartmentName, 14, true)
      .WithBorderBottom())
         .WithContent(c => c
    .WithInfoFields(f => f
         .WithColumns(2)
        .AddField("Nama Pasien", PlaceholderKeys.PatientName, 0)
       .AddField("No. RM", PlaceholderKeys.PatientMRN, 1))
    .WithImageGrid(2, 2, 4, 10)
             .WithResults("Hasil Pemeriksaan"))
         .WithFooter(f => f
     .WithDateLocation("Jakarta")
     .WithSignature("Dokter Pemeriksa"))
         .Build();

 // Assert
     Assert.Equal("Test Template", template.Name);
  Assert.NotNull(template.Header);
   Assert.Equal(2, template.Header.Lines.Count);
 Assert.NotNull(template.Content);
     Assert.Equal(2, template.Content.InfoFields.Fields.Count);
        Assert.NotNull(template.Footer);
    }

    [Fact]
    public async Task ReportBuilder_ShouldGeneratePdf()
    {
     // Arrange
 var template = new TemplateBuilder()
 .WithName("PDF Test")
      .WithPaperSize(PaperSize.A4)
         .WithHeader(h => h
        .AddLine("Test Institution", 16, true))
            .WithContent(c => c
     .WithResults())
            .Build();

       var generator = new PdfReportGenerator();
 var builder = new ReportBuilder.Core.Builder.ReportBuilder(template, generator);

        // Act
        var result = await builder
 .WithInstitution("Test Hospital")
    .WithPatient("John Doe", "MRN-001", DateTime.Now.AddYears(-30), "123 Test St")
         .WithExamination("Test Exam", DateTime.Now, "Room 1", "Test notes", "Normal findings")
        .WithDoctor("Dr. Test")
            .WithCity("Jakarta")
       .GeneratePdfAsync();

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.PdfData);
        Assert.True(result.PdfData.Length > 0);
    }

    [Fact]
    public void PlaceholderResolver_ShouldResolvePatientData()
    {
     // Arrange
        var resolver = new PlaceholderResolver();
 var data = new ReportData
     {
    Patient = new PatientData
   {
       Name = "John Doe",
 MedicalRecordNumber = "MRN-12345"
     }
        };

        // Act
     var name = resolver.Resolve(PlaceholderKeys.PatientName, data);
     var mrn = resolver.Resolve(PlaceholderKeys.PatientMRN, data);

        // Assert
        Assert.Equal("John Doe", name);
      Assert.Equal("MRN-12345", mrn);
    }

  [Fact]
    public void PlaceholderResolver_ShouldResolveTextWithMultiplePlaceholders()
  {
    // Arrange
    var resolver = new PlaceholderResolver();
     var data = new ReportData
        {
    Patient = new PatientData { Name = "Jane" },
  Doctor = new DoctorData { Name = "Dr. Smith" }
        };

        // Act
      var text = resolver.ResolveText("Patient: {PatientName}, Doctor: {DoctorName}", data);

 // Assert
     Assert.Equal("Patient: Jane, Doctor: Dr. Smith", text);
    }

    [Fact]
    public void ReportBuilder_FluentApi_ShouldBuildCorrectData()
    {
        // Arrange
        var template = new ReportTemplate();
   var generator = new PdfReportGenerator();
   var builder = new ReportBuilder.Core.Builder.ReportBuilder(template, generator);

    // Act
        builder
      .WithInstitution("Hospital A", "Address", "Phone", "email@test.com", "Radiology")
            .WithPatient("Patient Name", "MRN-001", new DateTime(1990, 1, 1), "Patient Address")
  .WithExamination("USG", DateTime.Today, "Room 1", "Notes", "Results")
      .WithDoctor("Dr. Test", "SIP-123")
    .WithCity("Jakarta")
  .WithCustomData("CustomField", "CustomValue");

        var data = builder.GetData();

        // Assert
        Assert.Equal("Hospital A", data.Institution.Name);
   Assert.Equal("Patient Name", data.Patient.Name);
     Assert.Equal("USG", data.Examination.Name);
 Assert.Equal("Dr. Test", data.Doctor.Name);
        Assert.Equal("Jakarta", data.CityName);
   Assert.Equal("CustomValue", data.CustomData["CustomField"]);
    }

    [Fact]
    public void ReportBuilder_ShouldHandleImages()
    {
        // Arrange
        var template = new ReportTemplate();
        var generator = new PdfReportGenerator();
        var builder = new ReportBuilder.Core.Builder.ReportBuilder(template, generator);
        var testImage = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header

        // Act
      builder.AddImage(testImage, "Test Image");
        var data = builder.GetData();

        // Assert
        Assert.Single(data.Images);
        Assert.Equal("Test Image", data.Images[0].Caption);
  }
}
