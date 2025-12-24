# ReportBuilder

A customizable PDF report builder library for .NET 8, supporting WinUI 3, SQLite storage, and fluent API design.

## Features

- ?? **Customizable Templates** - Create and manage report templates with header, content, and footer sections
- ?? **Visual Template Designer** - WinUI 3 control for designing templates with live preview
- ?? **Fluent API** - Easy-to-use builder pattern for generating reports
- ?? **SQLite Storage** - Persistent storage for templates and settings
- ?? **PDF Generation** - High-quality PDF output using QuestPDF
- ??? **Print Support** - Direct printing capabilities
- ?? **Placeholder System** - Dynamic data binding with customizable placeholders

## Installation

### From NuGet (Coming Soon)

```bash
dotnet add package ReportBuilder.Core
dotnet add package ReportBuilder.WinUI
```

### From Source

1. Clone the repository
2. Add project references to your solution

## Quick Start

### 1. Initialize the Database

```csharp
using ReportBuilder.Core.Storage;

var dbPath = Path.Combine(Environment.GetFolderPath(
    Environment.SpecialFolder.LocalApplicationData), "MyApp", "reportbuilder.db");

var unitOfWork = new SqliteUnitOfWork(dbPath);
await unitOfWork.InitializeDatabaseAsync();
```

### 2. Create a Template Programmatically

```csharp
using ReportBuilder.Core.Builder;
using ReportBuilder.Core.Models;

var template = new TemplateBuilder()
    .WithName("Medical Report")
    .WithPaperSize(PaperSize.A4)
    .WithHeader(h => h
        .WithLeftLogo(80, 80)
        .WithRightLogo(80, 80)
      .AddPlaceholderLine(PlaceholderKeys.InstitutionName, 16, bold: true)
        .AddPlaceholderLine(PlaceholderKeys.DepartmentName, 14)
        .WithBorderBottom())
    .WithContent(c => c
        .WithInfoFields(f => f
            .WithColumns(2)
            .AddField("Patient Name", PlaceholderKeys.PatientName, column: 0)
         .AddField("MRN", PlaceholderKeys.PatientMRN, column: 1))
        .WithImageGrid(columns: 2, rows: 2, maxImages: 4)
        .WithResults("Examination Results"))
    .WithFooter(f => f
        .WithDateLocation("Jakarta")
  .WithSignature("Examining Doctor"))
    .Build();
```

### 3. Generate a PDF Report

```csharp
using ReportBuilder.Core.Builder;

var factory = new ReportBuilderFactory(unitOfWork);
var builder = await factory.CreateFromTemplateAsync(templateId);

var result = await builder
    .WithInstitution("Sample Hospital", "123 Medical St")
    .WithPatient("John Doe", "MRN-001234", new DateTime(1990, 5, 15), "123 Patient St")
    .WithExamination("Ultrasound", DateTime.Now, "Room 1", "Clinical notes", "Normal findings")
    .WithDoctor("Dr. Jane Smith, Sp.Rad")
    .WithCity("Jakarta")
.AddImage("/path/to/image1.jpg")
    .AddImage("/path/to/image2.jpg")
    .GeneratePdfAsync("/output/report.pdf");

if (result.Success)
{
    Console.WriteLine($"PDF generated in {result.GenerationTime.TotalMilliseconds}ms");
}
```

### 4. Use the Template Designer (WinUI 3)

```xaml
<controls:TemplateDesigner x:Name="Designer"/>
```

```csharp
await Designer.InitializeAsync(databasePath);
```

## Project Structure

```
ReportBuilder/
??? src/
?   ??? ReportBuilder.Core/     # Core library (.NET 8)
?   ?   ??? Models/  # Data models
?   ?   ??? Storage/          # SQLite repository
?   ?   ??? Services/   # PDF generation
?   ?   ??? Builder/        # Fluent API builders
?   ?
? ??? ReportBuilder.WinUI/  # WinUI 3 UI components
???? Controls/         # Designer controls
?
??? tests/
?   ??? ReportBuilder.Tests/      # Unit tests
?
??? samples/
    ??? ReportBuilder.Demo/         # Demo application
```

## Available Placeholders

### Institution
- `{InstitutionName}` - Institution name
- `{InstitutionAddress}` - Address
- `{InstitutionPhone}` - Phone number
- `{InstitutionEmail}` - Email
- `{DepartmentName}` - Department name

### Patient
- `{PatientName}` - Patient full name
- `{PatientMRN}` - Medical record number
- `{PatientBirthDate}` - Date of birth
- `{PatientAge}` - Age
- `{PatientAddress}` - Address

### Examination
- `{ExamName}` - Examination name
- `{ExamDate}` - Examination date
- `{ExamRoom}` - Room name
- `{ClinicalNotes}` - Clinical notes
- `{ExamResult}` - Examination results

### Doctor
- `{DoctorName}` - Doctor name
- `{DoctorCredentials}` - Credentials/license number

### Date/Time
- `{CurrentDate}` - Current date
- `{PrintDate}` - Print date

## Requirements

- .NET 8.0 or later
- Windows 10 version 1809 (build 17763) or later for WinUI 3

## Dependencies

- **QuestPDF** - PDF generation (MIT License, free for community use)
- **Microsoft.Data.Sqlite** - SQLite database
- **Microsoft.WindowsAppSDK** - WinUI 3 (for UI components)

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Roadmap

- [ ] Export/Import templates as JSON
- [ ] Template sharing marketplace
- [ ] More paper sizes and custom sizes
- [ ] Watermark support
- [ ] Digital signature support
- [ ] Multi-page report support
- [ ] Report history/versioning
- [ ] Cloud storage integration
