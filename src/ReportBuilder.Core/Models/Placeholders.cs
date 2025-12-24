namespace ReportBuilder.Core.Models;

/// <summary>
/// Predefined placeholder keys for dynamic data binding.
/// </summary>
public static class PlaceholderKeys
{
    // Institution/Header placeholders
    public const string InstitutionName = "{InstitutionName}";
    public const string InstitutionAddress = "{InstitutionAddress}";
    public const string InstitutionPhone = "{InstitutionPhone}";
    public const string InstitutionFax = "{InstitutionFax}";
    public const string InstitutionEmail = "{InstitutionEmail}";
    public const string InstitutionWebsite = "{InstitutionWebsite}";
    public const string DepartmentName = "{DepartmentName}";
    public const string RoomName = "{RoomName}";

    // Patient placeholders
    public const string PatientName = "{PatientName}";
    public const string PatientMRN = "{PatientMRN}";  // Medical Record Number
    public const string PatientBirthDate = "{PatientBirthDate}";
    public const string PatientAge = "{PatientAge}";
    public const string PatientGender = "{PatientGender}";
    public const string PatientAddress = "{PatientAddress}";
    public const string PatientPhone = "{PatientPhone}";
    public const string PatientInsurance = "{PatientInsurance}";

    // Examination placeholders
    public const string ExamName = "{ExamName}";
    public const string ExamDate = "{ExamDate}";
    public const string ExamTime = "{ExamTime}";
    public const string ExamDateTime = "{ExamDateTime}";
    public const string ExamRoom = "{ExamRoom}";
    public const string ClinicalNotes = "{ClinicalNotes}";
    public const string ExamResult = "{ExamResult}";
    public const string ExamConclusion = "{ExamConclusion}";
    public const string ExamRecommendation = "{ExamRecommendation}";

    // Doctor/Staff placeholders
    public const string DoctorName = "{DoctorName}";
    public const string DoctorCredentials = "{DoctorCredentials}";
 public const string DoctorSpecialty = "{DoctorSpecialty}";
    public const string TechnicianName = "{TechnicianName}";
public const string OperatorName = "{OperatorName}";

    // Date/Time placeholders
    public const string CurrentDate = "{CurrentDate}";
    public const string CurrentTime = "{CurrentTime}";
    public const string CurrentDateTime = "{CurrentDateTime}";
  public const string PrintDate = "{PrintDate}";

 // Document placeholders
    public const string PageNumber = "{PageNumber}";
    public const string TotalPages = "{TotalPages}";
 public const string DocumentId = "{DocumentId}";
}

/// <summary>
/// Placeholder definition for database storage and UI display.
/// </summary>
public class PlaceholderDefinition
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public PlaceholderDataType DataType { get; set; } = PlaceholderDataType.String;
    public string? DefaultValue { get; set; }
    public string? Format { get; set; }  // For dates/numbers
    public bool IsSystem { get; set; } = true;  // System-defined vs user-defined
}

/// <summary>
/// Data types for placeholders.
/// </summary>
public enum PlaceholderDataType
{
    String = 0,
    Integer = 1,
    Decimal = 2,
    Date = 3,
    DateTime = 4,
    Boolean = 5,
    Image = 6
}

/// <summary>
/// Categories for organizing placeholders.
/// </summary>
public static class PlaceholderCategories
{
    public const string Header = "Header";
    public const string Patient = "Patient";
    public const string Examination = "Examination";
    public const string Doctor = "Doctor";
    public const string DateTime = "DateTime";
    public const string Document = "Document";
    public const string Custom = "Custom";
}
