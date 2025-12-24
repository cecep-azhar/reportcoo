using ReportBuilder.Core.Models;
using ReportBuilder.Core.Services;
using ReportBuilder.Core.Storage;

namespace ReportBuilder.Core.Builder;

/// <summary>
/// Factory for creating ReportBuilder instances.
/// </summary>
public class ReportBuilderFactory
{
 private readonly IReportBuilderUnitOfWork _unitOfWork;
    private readonly IReportGenerator _reportGenerator;

    public ReportBuilderFactory(IReportBuilderUnitOfWork unitOfWork, IReportGenerator? reportGenerator = null)
  {
        _unitOfWork = unitOfWork;
     _reportGenerator = reportGenerator ?? new PdfReportGenerator();
    }

    /// <summary>
    /// Creates a ReportBuilder from a template ID.
    /// </summary>
    public async Task<ReportBuilder> CreateFromTemplateAsync(int templateId)
    {
        var template = await _unitOfWork.Templates.GetByIdAsync(templateId)
            ?? throw new InvalidOperationException($"Template with ID {templateId} not found");
        
        return new ReportBuilder(template, _reportGenerator);
    }

    /// <summary>
    /// Creates a ReportBuilder from a template name.
    /// </summary>
    public async Task<ReportBuilder> CreateFromTemplateAsync(string templateName)
    {
        var template = await _unitOfWork.Templates.GetByNameAsync(templateName)
            ?? throw new InvalidOperationException($"Template '{templateName}' not found");
    
        return new ReportBuilder(template, _reportGenerator);
    }

    /// <summary>
    /// Creates a ReportBuilder from the default template.
    /// </summary>
 public async Task<ReportBuilder> CreateFromDefaultTemplateAsync()
  {
        var template = await _unitOfWork.Templates.GetDefaultAsync()
      ?? throw new InvalidOperationException("No default template found");
        
        return new ReportBuilder(template, _reportGenerator);
    }

 /// <summary>
    /// Creates a ReportBuilder with a new empty template.
    /// </summary>
    public ReportBuilder CreateNew()
    {
        return new ReportBuilder(new ReportTemplate(), _reportGenerator);
  }

    /// <summary>
    /// Creates a ReportBuilder with the provided template.
    /// </summary>
    public ReportBuilder CreateWithTemplate(ReportTemplate template)
    {
      return new ReportBuilder(template, _reportGenerator);
    }
}

/// <summary>
/// Fluent builder for creating reports.
/// </summary>
public class ReportBuilder
{
    private readonly ReportTemplate _template;
    private readonly IReportGenerator _generator;
    private readonly ReportData _data;

    internal ReportBuilder(ReportTemplate template, IReportGenerator generator)
    {
    _template = template;
        _generator = generator;
   _data = new ReportData();
    }

    #region Institution Data

    /// <summary>
    /// Sets the institution data for the report.
    /// </summary>
    public ReportBuilder WithInstitution(InstitutionData institution)
    {
      _data.Institution = institution;
        return this;
    }

    /// <summary>
    /// Sets the institution data using individual parameters.
    /// </summary>
    public ReportBuilder WithInstitution(
        string name,
        string? address = null,
        string? phone = null,
        string? email = null,
        string? department = null,
     byte[]? logoLeft = null,
        byte[]? logoRight = null)
    {
        _data.Institution = new InstitutionData
        {
         Name = name,
            Address = address ?? string.Empty,
        Phone = phone ?? string.Empty,
            Email = email ?? string.Empty,
   Department = department ?? string.Empty,
      LogoLeft = logoLeft,
         LogoRight = logoRight
        };
        return this;
    }

    /// <summary>
    /// Sets the left logo from a file path.
    /// </summary>
    public ReportBuilder WithLeftLogo(string filePath)
    {
        if (File.Exists(filePath))
        {
     _data.Institution = _data.Institution with { LogoLeft = File.ReadAllBytes(filePath) };
        }
        return this;
    }

    /// <summary>
  /// Sets the right logo from a file path.
    /// </summary>
    public ReportBuilder WithRightLogo(string filePath)
    {
        if (File.Exists(filePath))
        {
            _data.Institution = _data.Institution with { LogoRight = File.ReadAllBytes(filePath) };
        }
    return this;
 }

 #endregion

    #region Patient Data

    /// <summary>
    /// Sets the patient data for the report.
    /// </summary>
public ReportBuilder WithPatient(PatientData patient)
    {
      _data.Patient = patient;
        return this;
    }

    /// <summary>
    /// Sets the patient data using individual parameters.
    /// </summary>
    public ReportBuilder WithPatient(
        string name,
        string medicalRecordNumber,
        DateTime? birthDate = null,
    string? address = null,
        string? gender = null,
        string? phone = null)
    {
        _data.Patient = new PatientData
    {
        Name = name,
  MedicalRecordNumber = medicalRecordNumber,
       BirthDate = birthDate,
   Address = address ?? string.Empty,
         Gender = gender ?? string.Empty,
            Phone = phone ?? string.Empty
        };
     return this;
    }

    #endregion

 #region Examination Data

    /// <summary>
    /// Sets the examination data for the report.
/// </summary>
    public ReportBuilder WithExamination(ExaminationData examination)
  {
 _data.Examination = examination;
        return this;
    }

    /// <summary>
    /// Sets the examination data using individual parameters.
    /// </summary>
    public ReportBuilder WithExamination(
      string name,
        DateTime? date = null,
        string? room = null,
        string? clinicalNotes = null,
        string? result = null,
    string? conclusion = null,
  string? recommendation = null)
    {
        _data.Examination = new ExaminationData
  {
   Name = name,
         Date = date ?? DateTime.Now,
            Room = room ?? string.Empty,
   ClinicalNotes = clinicalNotes ?? string.Empty,
 Result = result ?? string.Empty,
      Conclusion = conclusion ?? string.Empty,
            Recommendation = recommendation ?? string.Empty
        };
        return this;
    }

    /// <summary>
    /// Sets the examination result.
    /// </summary>
    public ReportBuilder WithResult(string result)
    {
        _data.Examination = _data.Examination with { Result = result };
      return this;
    }

    /// <summary>
 /// Sets the clinical notes.
    /// </summary>
    public ReportBuilder WithClinicalNotes(string clinicalNotes)
    {
    _data.Examination = _data.Examination with { ClinicalNotes = clinicalNotes };
        return this;
    }

    #endregion

    #region Doctor Data

    /// <summary>
    /// Sets the doctor data for the report.
    /// </summary>
    public ReportBuilder WithDoctor(DoctorData doctor)
    {
     _data.Doctor = doctor;
    return this;
    }

    /// <summary>
    /// Sets the doctor data using individual parameters.
    /// </summary>
    public ReportBuilder WithDoctor(string name, string? credentials = null, string? specialty = null)
    {
    _data.Doctor = new DoctorData(name, credentials ?? string.Empty, specialty ?? string.Empty);
        return this;
    }

    #endregion

  #region Images

    /// <summary>
    /// Adds an image to the report from byte array.
    /// </summary>
    public ReportBuilder AddImage(byte[] imageData, string? caption = null)
    {
_data.Images.Add(new ReportImage
     {
       Order = _data.Images.Count + 1,
    Data = imageData,
            Caption = caption,
    CapturedAt = DateTime.Now
    });
     return this;
    }

    /// <summary>
    /// Adds an image to the report from file path.
    /// </summary>
    public ReportBuilder AddImage(string filePath, string? caption = null)
    {
      if (File.Exists(filePath))
        {
            _data.Images.Add(new ReportImage
{
   Order = _data.Images.Count + 1,
                FilePath = filePath,
            Caption = caption,
            CapturedAt = DateTime.Now
   });
        }
        return this;
    }

    /// <summary>
    /// Adds multiple images from file paths.
  /// </summary>
 public ReportBuilder AddImages(IEnumerable<string> filePaths)
    {
   foreach (var path in filePaths)
        {
       AddImage(path);
    }
        return this;
    }

    /// <summary>
    /// Adds multiple images from byte arrays.
    /// </summary>
    public ReportBuilder AddImages(IEnumerable<byte[]> images)
    {
        foreach (var image in images)
    {
            AddImage(image);
  }
      return this;
    }

    /// <summary>
    /// Clears all images.
    /// </summary>
    public ReportBuilder ClearImages()
  {
        _data.Images.Clear();
   return this;
    }

    #endregion

    #region Custom Data

    /// <summary>
    /// Adds custom data for placeholder resolution.
    /// </summary>
    public ReportBuilder WithCustomData(string key, object value)
    {
        _data.CustomData[key] = value;
     return this;
    }

    /// <summary>
    /// Sets the city name for the footer.
    /// </summary>
    public ReportBuilder WithCity(string cityName)
    {
        _data.CityName = cityName;
    return this;
    }

    #endregion

    #region Generation

    /// <summary>
    /// Generates the PDF report.
    /// </summary>
    public async Task<ReportGenerationResult> GeneratePdfAsync()
    {
  _data.GeneratedAt = DateTime.Now;
        return await _generator.GeneratePdfAsync(_template, _data);
    }

 /// <summary>
    /// Generates the PDF report and saves to a file.
  /// </summary>
  public async Task<ReportGenerationResult> GeneratePdfAsync(string outputPath)
    {
        _data.GeneratedAt = DateTime.Now;
        return await _generator.GeneratePdfToFileAsync(_template, _data, outputPath);
    }

 /// <summary>
    /// Gets a preview image of the report.
  /// </summary>
    public async Task<byte[]?> GetPreviewAsync(int width = 800)
    {
        _data.GeneratedAt = DateTime.Now;
     return await _generator.GetPreviewImageAsync(_template, _data, width);
    }

    /// <summary>
    /// Prints the report directly.
    /// </summary>
    public async Task<PrintResult> PrintAsync(string printerName, int copies = 1)
    {
        _data.GeneratedAt = DateTime.Now;
        return await _generator.PrintAsync(_template, _data, printerName, copies);
    }

    /// <summary>
    /// Gets the underlying template for modification.
    /// </summary>
    public ReportTemplate GetTemplate() => _template;

    /// <summary>
    /// Gets the report data.
    /// </summary>
    public ReportData GetData() => _data;

 #endregion
}
