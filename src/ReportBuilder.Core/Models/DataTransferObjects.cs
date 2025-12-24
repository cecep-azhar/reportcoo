namespace ReportBuilder.Core.Models;

/// <summary>
/// Data transfer object for patient data.
/// </summary>
public record PatientData
{
    public string Name { get; init; } = string.Empty;
    public string MedicalRecordNumber { get; init; } = string.Empty;
    public DateTime? BirthDate { get; init; }
    public int? Age { get; init; }
    public string Gender { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Insurance { get; init; } = string.Empty;

    public PatientData() { }

    public PatientData(string name, string mrn, DateTime? birthDate, string address)
    {
        Name = name;
      MedicalRecordNumber = mrn;
  BirthDate = birthDate;
  Address = address;
    }
}

/// <summary>
/// Data transfer object for examination data.
/// </summary>
public record ExaminationData
{
    public string Name { get; init; } = string.Empty;
    public DateTime Date { get; init; } = DateTime.Now;
    public TimeSpan? Time { get; init; }
    public string Room { get; init; } = string.Empty;
    public string ClinicalNotes { get; init; } = string.Empty;
    public string Result { get; init; } = string.Empty;
  public string Conclusion { get; init; } = string.Empty;
    public string Recommendation { get; init; } = string.Empty;

    public ExaminationData() { }

    public ExaminationData(string name, DateTime date, string room, string clinicalNotes, string result)
    {
        Name = name;
Date = date;
        Room = room;
        ClinicalNotes = clinicalNotes;
        Result = result;
    }
}

/// <summary>
/// Data transfer object for doctor/medical staff data.
/// </summary>
public record DoctorData
{
    public string Name { get; init; } = string.Empty;
    public string Credentials { get; init; } = string.Empty;  // e.g., SIP, license number
    public string Specialty { get; init; } = string.Empty;
    public byte[]? SignatureImage { get; init; }

    public DoctorData() { }

    public DoctorData(string name, string credentials = "", string specialty = "")
    {
        Name = name;
        Credentials = credentials;
        Specialty = specialty;
    }
}

/// <summary>
/// Data transfer object for institution data.
/// </summary>
public record InstitutionData
{
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Fax { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Website { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public byte[]? LogoLeft { get; init; }
    public byte[]? LogoRight { get; init; }

    public InstitutionData() { }
}

/// <summary>
/// Complete report data containing all information needed to generate a report.
/// </summary>
public class ReportData
{
    public InstitutionData Institution { get; set; } = new();
    public PatientData Patient { get; set; } = new();
    public ExaminationData Examination { get; set; } = new();
    public DoctorData Doctor { get; set; } = new();
    public List<ReportImage> Images { get; set; } = new();
    public Dictionary<string, object> CustomData { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public string? CityName { get; set; }
}

/// <summary>
/// Represents an image to be included in the report.
/// </summary>
public class ReportImage
{
    public int Order { get; set; }
    public byte[]? Data { get; set; }
    public string? FilePath { get; set; }
    public string? Caption { get; set; }
    public DateTime? CapturedAt { get; set; }
}
