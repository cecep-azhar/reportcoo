using Microsoft.Data.Sqlite;
using ReportBuilder.Core.Models;

namespace ReportBuilder.Core.Storage;

/// <summary>
/// Unit of work implementation for SQLite repositories.
/// </summary>
public class SqliteUnitOfWork : IReportBuilderUnitOfWork
{
    private readonly string _connectionString;
    private readonly string _databasePath;
    private bool _disposed;

public ITemplateRepository Templates { get; }
    public ISettingsRepository Settings { get; }
    public IPlaceholderRepository Placeholders { get; }

    public SqliteUnitOfWork(string databasePath)
    {
        _databasePath = databasePath;
      _connectionString = $"Data Source={databasePath}";

        Templates = new SqliteTemplateRepository(_connectionString);
    Settings = new SqliteSettingsRepository(_connectionString);
        Placeholders = new SqlitePlaceholderRepository(_connectionString);
    }

    /// <summary>
    /// Initializes the database by creating tables if they don't exist.
  /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        // Ensure directory exists
        var directory = Path.GetDirectoryName(_databasePath);
    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
 {
       Directory.CreateDirectory(directory);
        }

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

    // Create tables
        await using (var command = new SqliteCommand(DatabaseSchema.CreateTablesScript, connection))
      {
 await command.ExecuteNonQueryAsync();
        }

        // Insert default placeholders
        await using (var command = new SqliteCommand(DatabaseSchema.InsertDefaultPlaceholders, connection))
      {
            await command.ExecuteNonQueryAsync();
        }

        // Create default template if none exists
        await CreateDefaultTemplateIfNotExistsAsync();
    }

    /// <summary>
    /// Creates a backup of the database.
    /// </summary>
    public async Task<bool> BackupDatabaseAsync(string backupPath)
    {
        try
        {
    if (File.Exists(_databasePath))
            {
     var directory = Path.GetDirectoryName(backupPath);
     if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
{
   Directory.CreateDirectory(directory);
  }

        await using var sourceConnection = new SqliteConnection(_connectionString);
    await sourceConnection.OpenAsync();

      await using var backupConnection = new SqliteConnection($"Data Source={backupPath}");
      await backupConnection.OpenAsync();

         sourceConnection.BackupDatabase(backupConnection);
       return true;
      }
         return false;
   }
      catch
        {
 return false;
        }
    }

    private async Task CreateDefaultTemplateIfNotExistsAsync()
    {
        var existingDefault = await Templates.GetDefaultAsync();
        if (existingDefault != null) return;

      var defaultTemplate = CreateDefaultReportTemplate();
        await Templates.CreateAsync(defaultTemplate);
        await Templates.SetDefaultAsync(defaultTemplate.Id);
}

    private static ReportTemplate CreateDefaultReportTemplate()
    {
        return new ReportTemplate
        {
            Name = "Template Default",
            Description = "Template laporan default dengan header, info pasien, grid gambar, dan footer",
  PaperSize = PaperSize.A4,
    Orientation = PageOrientation.Portrait,
            Margins = new PageMargins(20f),
            IsDefault = true,
     IsActive = true,
            Header = new HeaderSection
 {
                IsVisible = true,
          Height = 100f,
     LeftLogo = new LogoPlacement { IsVisible = true, Width = 80, Height = 80 },
   RightLogo = new LogoPlacement { IsVisible = true, Width = 80, Height = 80 },
      Lines = new List<HeaderLine>
      {
            new() { Order = 1, PlaceholderKey = PlaceholderKeys.InstitutionName, FontSize = 16, IsBold = true, Alignment = TextAlignment.Center },
           new() { Order = 2, PlaceholderKey = PlaceholderKeys.DepartmentName, FontSize = 14, IsBold = true, Alignment = TextAlignment.Center },
              new() { Order = 3, PlaceholderKey = PlaceholderKeys.InstitutionAddress, FontSize = 10, Alignment = TextAlignment.Center },
  new() { Order = 4, PlaceholderKey = PlaceholderKeys.InstitutionPhone, FontSize = 9, Alignment = TextAlignment.Center }
                },
    ShowBorderBottom = true,
           BorderColor = "#000000",
     BorderThickness = 1f
  },
            Content = new ContentSection
            {
         InfoFields = new InfoFieldsLayout
                {
            IsVisible = true,
    ColumnsCount = 2,
  FontSize = 12f,
  Fields = new List<InfoField>
   {
            new() { Label = "Nama Pasien", PlaceholderKey = PlaceholderKeys.PatientName, Column = 0, Order = 1 },
        new() { Label = "Jenis Pemeriksaan", PlaceholderKey = PlaceholderKeys.ExamName, Column = 1, Order = 1 },
          new() { Label = "Tanggal Lahir", PlaceholderKey = PlaceholderKeys.PatientBirthDate, Column = 0, Order = 2 },
       new() { Label = "Keterangan Klinis", PlaceholderKey = PlaceholderKeys.ClinicalNotes, Column = 1, Order = 2 },
          new() { Label = "No. RM", PlaceholderKey = PlaceholderKeys.PatientMRN, Column = 0, Order = 3 },
  new() { Label = "Alamat", PlaceholderKey = PlaceholderKeys.PatientAddress, Column = 1, Order = 3 }
             }
 },
     ImageGrid = new ImageGridLayout
                {
     IsVisible = true,
    Columns = 2,
          Rows = 2,
        MaxImages = 4,
 ImageSpacing = 10f,
           ScaleMode = ImageScaleMode.Uniform
      },
   Results = new ResultSection
       {
 IsVisible = true,
         Label = "Hasil Pemeriksaan",
       PlaceholderKey = PlaceholderKeys.ExamResult,
    LabelBold = true,
    FontSize = 12f
   }
 },
         Footer = new FooterSection
            {
   IsVisible = true,
        Height = 120f,
     DateLocation = new DateLocationBlock
              {
      IsVisible = true,
 DateFormat = "dd MMMM yyyy",
           CultureCode = "id-ID",
     Position = HorizontalPosition.Right
 },
       Signature = new SignatureBlock
       {
        IsVisible = true,
          TitleLabel = "Dokter Pemeriksa",
           NamePlaceholder = PlaceholderKeys.DoctorName,
   SignatureSpaceHeight = 60f,
          Position = HorizontalPosition.Right
    }
            }
        };
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    if (_disposed) return;
 _disposed = true;
    }

    protected virtual ValueTask DisposeAsyncCore()
    {
        return ValueTask.CompletedTask;
    }
}
