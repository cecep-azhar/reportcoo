using System.Globalization;
using System.Text.RegularExpressions;
using ReportBuilder.Core.Models;

namespace ReportBuilder.Core.Services;

/// <summary>
/// Resolves placeholder keys to actual values from report data.
/// </summary>
public partial class PlaceholderResolver : IPlaceholderResolver
{
    private readonly Dictionary<string, Func<ReportData, string?>> _resolvers;

    public PlaceholderResolver()
    {
        _resolvers = new Dictionary<string, Func<ReportData, string?>>
        {
    // Institution
       [PlaceholderKeys.InstitutionName] = d => d.Institution.Name,
   [PlaceholderKeys.InstitutionAddress] = d => d.Institution.Address,
        [PlaceholderKeys.InstitutionPhone] = d => d.Institution.Phone,
            [PlaceholderKeys.InstitutionFax] = d => d.Institution.Fax,
   [PlaceholderKeys.InstitutionEmail] = d => d.Institution.Email,
      [PlaceholderKeys.InstitutionWebsite] = d => d.Institution.Website,
       [PlaceholderKeys.DepartmentName] = d => d.Institution.Department,
            [PlaceholderKeys.RoomName] = d => d.Examination.Room,

     // Patient
       [PlaceholderKeys.PatientName] = d => d.Patient.Name,
   [PlaceholderKeys.PatientMRN] = d => d.Patient.MedicalRecordNumber,
            [PlaceholderKeys.PatientBirthDate] = d => FormatDate(d.Patient.BirthDate),
            [PlaceholderKeys.PatientAge] = d => d.Patient.Age?.ToString() ?? CalculateAge(d.Patient.BirthDate)?.ToString(),
 [PlaceholderKeys.PatientGender] = d => d.Patient.Gender,
        [PlaceholderKeys.PatientAddress] = d => d.Patient.Address,
            [PlaceholderKeys.PatientPhone] = d => d.Patient.Phone,
    [PlaceholderKeys.PatientInsurance] = d => d.Patient.Insurance,

       // Examination
            [PlaceholderKeys.ExamName] = d => d.Examination.Name,
            [PlaceholderKeys.ExamDate] = d => FormatDate(d.Examination.Date),
       [PlaceholderKeys.ExamTime] = d => d.Examination.Time?.ToString(@"hh\:mm"),
      [PlaceholderKeys.ExamDateTime] = d => FormatDateTime(d.Examination.Date, d.Examination.Time),
   [PlaceholderKeys.ExamRoom] = d => d.Examination.Room,
  [PlaceholderKeys.ClinicalNotes] = d => d.Examination.ClinicalNotes,
     [PlaceholderKeys.ExamResult] = d => d.Examination.Result,
            [PlaceholderKeys.ExamConclusion] = d => d.Examination.Conclusion,
     [PlaceholderKeys.ExamRecommendation] = d => d.Examination.Recommendation,

         // Doctor
          [PlaceholderKeys.DoctorName] = d => d.Doctor.Name,
      [PlaceholderKeys.DoctorCredentials] = d => d.Doctor.Credentials,
            [PlaceholderKeys.DoctorSpecialty] = d => d.Doctor.Specialty,

            // DateTime
            [PlaceholderKeys.CurrentDate] = _ => FormatDate(DateTime.Now),
            [PlaceholderKeys.CurrentTime] = _ => DateTime.Now.ToString("HH:mm"),
     [PlaceholderKeys.CurrentDateTime] = _ => DateTime.Now.ToString("dd MMMM yyyy HH:mm", new CultureInfo("id-ID")),
  [PlaceholderKeys.PrintDate] = d => FormatDate(d.GeneratedAt),
        };
    }

    public string? Resolve(string placeholderKey, ReportData data)
    {
        if (string.IsNullOrEmpty(placeholderKey))
   return null;

        // Check predefined resolvers
        if (_resolvers.TryGetValue(placeholderKey, out var resolver))
     {
         return resolver(data);
        }

     // Check custom data
      var key = placeholderKey.Trim('{', '}');
        if (data.CustomData.TryGetValue(key, out var customValue))
        {
            return customValue?.ToString();
        }

        // Return placeholder as-is if not found (or empty string)
        return string.Empty;
    }

    public string ResolveText(string text, ReportData data)
    {
if (string.IsNullOrEmpty(text))
  return string.Empty;

   return PlaceholderPattern().Replace(text, match =>
        {
        var placeholder = match.Value;
return Resolve(placeholder, data) ?? placeholder;
   });
    }

    private static string? FormatDate(DateTime? date, string format = "dd MMMM yyyy")
    {
     return date?.ToString(format, new CultureInfo("id-ID"));
    }

    private static string FormatDateTime(DateTime date, TimeSpan? time)
    {
        var culture = new CultureInfo("id-ID");
    if (time.HasValue)
        {
            var dateTime = date.Date.Add(time.Value);
            return dateTime.ToString("dd MMMM yyyy HH:mm", culture);
    }
        return date.ToString("dd MMMM yyyy", culture);
    }

    private static int? CalculateAge(DateTime? birthDate)
    {
   if (!birthDate.HasValue) return null;
        
        var today = DateTime.Today;
        var age = today.Year - birthDate.Value.Year;
        if (birthDate.Value.Date > today.AddYears(-age)) age--;
      return age;
    }

    [GeneratedRegex(@"\{[^}]+\}")]
 private static partial Regex PlaceholderPattern();
}
