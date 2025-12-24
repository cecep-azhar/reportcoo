namespace ReportBuilder.Core.Storage;

/// <summary>
/// SQL schema for the ReportBuilder SQLite database.
/// </summary>
public static class DatabaseSchema
{
    public const string CreateTablesScript = @"
        -- Templates table
        CREATE TABLE IF NOT EXISTS Templates (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Name TEXT NOT NULL,
            Description TEXT,
            PaperSize INTEGER NOT NULL DEFAULT 0,
            Orientation INTEGER NOT NULL DEFAULT 0,
   MarginTop REAL NOT NULL DEFAULT 20,
            MarginBottom REAL NOT NULL DEFAULT 20,
          MarginLeft REAL NOT NULL DEFAULT 20,
    MarginRight REAL NOT NULL DEFAULT 20,
            IsDefault INTEGER NOT NULL DEFAULT 0,
       IsActive INTEGER NOT NULL DEFAULT 1,
          CreatedAt TEXT NOT NULL,
  UpdatedAt TEXT NOT NULL
  );

        -- Header sections
        CREATE TABLE IF NOT EXISTS HeaderSections (
     Id INTEGER PRIMARY KEY AUTOINCREMENT,
   TemplateId INTEGER NOT NULL UNIQUE,
        IsVisible INTEGER NOT NULL DEFAULT 1,
 Height REAL NOT NULL DEFAULT 100,
            LeftLogoVisible INTEGER DEFAULT 1,
            LeftLogoPath TEXT,
            LeftLogoData BLOB,
            LeftLogoWidth REAL DEFAULT 80,
            LeftLogoHeight REAL DEFAULT 80,
   LeftLogoVerticalAlign INTEGER DEFAULT 1,
            RightLogoVisible INTEGER DEFAULT 1,
   RightLogoPath TEXT,
     RightLogoData BLOB,
      RightLogoWidth REAL DEFAULT 80,
  RightLogoHeight REAL DEFAULT 80,
   RightLogoVerticalAlign INTEGER DEFAULT 1,
            ShowBorderBottom INTEGER DEFAULT 1,
            BorderColor TEXT DEFAULT '#000000',
      BorderThickness REAL DEFAULT 1,
   BackgroundColor TEXT,
            FOREIGN KEY (TemplateId) REFERENCES Templates(Id) ON DELETE CASCADE
        );

        -- Header lines
        CREATE TABLE IF NOT EXISTS HeaderLines (
     Id INTEGER PRIMARY KEY AUTOINCREMENT,
            HeaderSectionId INTEGER NOT NULL,
          LineOrder INTEGER NOT NULL,
          Text TEXT,
  PlaceholderKey TEXT,
            FontSize REAL DEFAULT 12,
     IsBold INTEGER DEFAULT 0,
            IsItalic INTEGER DEFAULT 0,
            IsUnderline INTEGER DEFAULT 0,
        FontFamily TEXT DEFAULT 'Arial',
    FontColor TEXT DEFAULT '#000000',
            Alignment INTEGER DEFAULT 1,
            IsVisible INTEGER DEFAULT 1,
    MarginTop REAL DEFAULT 2,
      MarginBottom REAL DEFAULT 2,
     FOREIGN KEY (HeaderSectionId) REFERENCES HeaderSections(Id) ON DELETE CASCADE
        );

        -- Content sections
        CREATE TABLE IF NOT EXISTS ContentSections (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        TemplateId INTEGER NOT NULL UNIQUE,
   FOREIGN KEY (TemplateId) REFERENCES Templates(Id) ON DELETE CASCADE
        );

        -- Info fields layout
   CREATE TABLE IF NOT EXISTS InfoFieldsLayouts (
     Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ContentSectionId INTEGER NOT NULL UNIQUE,
       IsVisible INTEGER DEFAULT 1,
 ColumnsCount INTEGER DEFAULT 2,
  FontSize REAL DEFAULT 12,
 FontFamily TEXT DEFAULT 'Arial',
         RowSpacing REAL DEFAULT 4,
    ColumnSpacing REAL DEFAULT 20,
 ShowBorder INTEGER DEFAULT 0,
            BorderColor TEXT DEFAULT '#CCCCCC',
            Padding REAL DEFAULT 8,
            FOREIGN KEY (ContentSectionId) REFERENCES ContentSections(Id) ON DELETE CASCADE
  );

  -- Info fields
        CREATE TABLE IF NOT EXISTS InfoFields (
   Id INTEGER PRIMARY KEY AUTOINCREMENT,
            InfoFieldsLayoutId INTEGER NOT NULL,
      Label TEXT NOT NULL,
            PlaceholderKey TEXT NOT NULL,
       ColumnIndex INTEGER DEFAULT 0,
            FieldOrder INTEGER NOT NULL,
            IsVisible INTEGER DEFAULT 1,
  LabelBold INTEGER DEFAULT 0,
      ValueBold INTEGER DEFAULT 0,
       Separator TEXT DEFAULT ':',
       LabelWidth REAL,
            FOREIGN KEY (InfoFieldsLayoutId) REFERENCES InfoFieldsLayouts(Id) ON DELETE CASCADE
   );

  -- Image grid settings
        CREATE TABLE IF NOT EXISTS ImageGridLayouts (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ContentSectionId INTEGER NOT NULL UNIQUE,
            IsVisible INTEGER DEFAULT 1,
  Columns INTEGER DEFAULT 2,
            Rows INTEGER DEFAULT 2,
            MaxImages INTEGER DEFAULT 4,
            ImageSpacing REAL DEFAULT 10,
          ShowImageBorder INTEGER DEFAULT 0,
   BorderColor TEXT DEFAULT '#CCCCCC',
      BorderThickness REAL DEFAULT 1,
  ScaleMode INTEGER DEFAULT 0,
            FixedWidth REAL,
   FixedHeight REAL,
   ShowImageNumbers INTEGER DEFAULT 0,
            BackgroundColor TEXT,
   CornerRadius REAL DEFAULT 0,
            FOREIGN KEY (ContentSectionId) REFERENCES ContentSections(Id) ON DELETE CASCADE
        );

     -- Result sections
     CREATE TABLE IF NOT EXISTS ResultSections (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ContentSectionId INTEGER NOT NULL UNIQUE,
            IsVisible INTEGER DEFAULT 1,
          Label TEXT DEFAULT 'Hasil Pemeriksaan',
            PlaceholderKey TEXT DEFAULT '{ExamResult}',
         FontSize REAL DEFAULT 12,
            FontFamily TEXT DEFAULT 'Arial',
            LabelBold INTEGER DEFAULT 1,
            ValueBold INTEGER DEFAULT 0,
       MinHeight REAL DEFAULT 50,
            ShowBorder INTEGER DEFAULT 0,
    BorderColor TEXT DEFAULT '#CCCCCC',
   Padding REAL DEFAULT 8,
    FOREIGN KEY (ContentSectionId) REFERENCES ContentSections(Id) ON DELETE CASCADE
        );

        -- Custom sections
        CREATE TABLE IF NOT EXISTS CustomSections (
   Id INTEGER PRIMARY KEY AUTOINCREMENT,
 ContentSectionId INTEGER NOT NULL,
     Name TEXT,
    SectionOrder INTEGER NOT NULL,
            IsVisible INTEGER DEFAULT 1,
   Type INTEGER DEFAULT 0,
 Content TEXT,
     PlaceholderKey TEXT,
   FontSize REAL DEFAULT 12,
    FontFamily TEXT DEFAULT 'Arial',
   IsBold INTEGER DEFAULT 0,
       Alignment INTEGER DEFAULT 0,
    MarginTop REAL DEFAULT 10,
   MarginBottom REAL DEFAULT 10,
      FOREIGN KEY (ContentSectionId) REFERENCES ContentSections(Id) ON DELETE CASCADE
        );

    -- Footer sections
 CREATE TABLE IF NOT EXISTS FooterSections (
   Id INTEGER PRIMARY KEY AUTOINCREMENT,
   TemplateId INTEGER NOT NULL UNIQUE,
            IsVisible INTEGER DEFAULT 1,
   Height REAL DEFAULT 120,
    ShowBorderTop INTEGER DEFAULT 0,
         BorderColor TEXT DEFAULT '#000000',
  BorderThickness REAL DEFAULT 1,
            BackgroundColor TEXT,
        FOREIGN KEY (TemplateId) REFERENCES Templates(Id) ON DELETE CASCADE
);

        -- Signature blocks
        CREATE TABLE IF NOT EXISTS SignatureBlocks (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
       FooterSectionId INTEGER NOT NULL UNIQUE,
          IsVisible INTEGER DEFAULT 1,
            TitleLabel TEXT DEFAULT 'Dokter Pemeriksa',
            NamePlaceholder TEXT DEFAULT '{DoctorName}',
   CredentialsPlaceholder TEXT DEFAULT '{DoctorCredentials}',
       SignatureSpaceHeight REAL DEFAULT 60,
       Position INTEGER DEFAULT 2,
            FontSize REAL DEFAULT 12,
  FontFamily TEXT DEFAULT 'Arial',
          ShowSignatureLine INTEGER DEFAULT 0,
            SignatureLineWidth REAL DEFAULT 150,
            FOREIGN KEY (FooterSectionId) REFERENCES FooterSections(Id) ON DELETE CASCADE
        );

        -- Date location blocks
        CREATE TABLE IF NOT EXISTS DateLocationBlocks (
     Id INTEGER PRIMARY KEY AUTOINCREMENT,
            FooterSectionId INTEGER NOT NULL UNIQUE,
            IsVisible INTEGER DEFAULT 1,
   CityName TEXT,
DateFormat TEXT DEFAULT 'dd MMMM yyyy',
      CultureCode TEXT DEFAULT 'id-ID',
            Position INTEGER DEFAULT 2,
   FontSize REAL DEFAULT 12,
    FontFamily TEXT DEFAULT 'Arial',
         CustomText TEXT,
            FOREIGN KEY (FooterSectionId) REFERENCES FooterSections(Id) ON DELETE CASCADE
        );

        -- Footer elements
    CREATE TABLE IF NOT EXISTS FooterElements (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
            FooterSectionId INTEGER NOT NULL,
 Name TEXT,
   ElementOrder INTEGER NOT NULL,
         Type INTEGER DEFAULT 0,
   Content TEXT,
            PlaceholderKey TEXT,
         Position INTEGER DEFAULT 1,
    FontSize REAL DEFAULT 10,
            FontFamily TEXT DEFAULT 'Arial',
      FontColor TEXT DEFAULT '#000000',
    IsBold INTEGER DEFAULT 0,
            IsItalic INTEGER DEFAULT 0,
 IsVisible INTEGER DEFAULT 1,
            FOREIGN KEY (FooterSectionId) REFERENCES FooterSections(Id) ON DELETE CASCADE
        );

        -- Placeholder definitions
        CREATE TABLE IF NOT EXISTS PlaceholderDefinitions (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
   Key TEXT NOT NULL UNIQUE,
            DisplayName TEXT NOT NULL,
          Category TEXT NOT NULL,
            DataType INTEGER DEFAULT 0,
 DefaultValue TEXT,
   Format TEXT,
  IsSystem INTEGER DEFAULT 1
        );

        -- Settings table
   CREATE TABLE IF NOT EXISTS Settings (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
      Key TEXT NOT NULL UNIQUE,
      Value TEXT,
            Category TEXT DEFAULT 'General',
            UpdatedAt TEXT NOT NULL
        );

        -- Create indexes
        CREATE INDEX IF NOT EXISTS idx_templates_name ON Templates(Name);
        CREATE INDEX IF NOT EXISTS idx_templates_default ON Templates(IsDefault);
        CREATE INDEX IF NOT EXISTS idx_headerlines_order ON HeaderLines(HeaderSectionId, LineOrder);
        CREATE INDEX IF NOT EXISTS idx_infofields_order ON InfoFields(InfoFieldsLayoutId, FieldOrder);
        CREATE INDEX IF NOT EXISTS idx_customsections_order ON CustomSections(ContentSectionId, SectionOrder);
        CREATE INDEX IF NOT EXISTS idx_footerelements_order ON FooterElements(FooterSectionId, ElementOrder);
   CREATE INDEX IF NOT EXISTS idx_settings_key ON Settings(Key);
    ";

    public const string InsertDefaultPlaceholders = @"
      INSERT OR IGNORE INTO PlaceholderDefinitions (Key, DisplayName, Category, DataType, IsSystem) VALUES
        -- Institution
        ('{InstitutionName}', 'Nama Institusi', 'Header', 0, 1),
        ('{InstitutionAddress}', 'Alamat Institusi', 'Header', 0, 1),
        ('{InstitutionPhone}', 'Telepon Institusi', 'Header', 0, 1),
        ('{InstitutionFax}', 'Fax Institusi', 'Header', 0, 1),
 ('{InstitutionEmail}', 'Email Institusi', 'Header', 0, 1),
        ('{InstitutionWebsite}', 'Website Institusi', 'Header', 0, 1),
    ('{DepartmentName}', 'Nama Departemen', 'Header', 0, 1),
   ('{RoomName}', 'Nama Ruangan', 'Header', 0, 1),
        -- Patient
('{PatientName}', 'Nama Pasien', 'Patient', 0, 1),
        ('{PatientMRN}', 'No. Rekam Medis', 'Patient', 0, 1),
        ('{PatientBirthDate}', 'Tanggal Lahir', 'Patient', 3, 1),
        ('{PatientAge}', 'Umur Pasien', 'Patient', 1, 1),
        ('{PatientGender}', 'Jenis Kelamin', 'Patient', 0, 1),
        ('{PatientAddress}', 'Alamat Pasien', 'Patient', 0, 1),
        ('{PatientPhone}', 'Telepon Pasien', 'Patient', 0, 1),
        ('{PatientInsurance}', 'Asuransi Pasien', 'Patient', 0, 1),
 -- Examination
        ('{ExamName}', 'Nama Pemeriksaan', 'Examination', 0, 1),
        ('{ExamDate}', 'Tanggal Pemeriksaan', 'Examination', 3, 1),
        ('{ExamTime}', 'Waktu Pemeriksaan', 'Examination', 0, 1),
   ('{ExamDateTime}', 'Tanggal & Waktu Pemeriksaan', 'Examination', 4, 1),
        ('{ExamRoom}', 'Ruang Pemeriksaan', 'Examination', 0, 1),
        ('{ClinicalNotes}', 'Keterangan Klinis', 'Examination', 0, 1),
        ('{ExamResult}', 'Hasil Pemeriksaan', 'Examination', 0, 1),
        ('{ExamConclusion}', 'Kesimpulan', 'Examination', 0, 1),
        ('{ExamRecommendation}', 'Rekomendasi', 'Examination', 0, 1),
   -- Doctor
        ('{DoctorName}', 'Nama Dokter', 'Doctor', 0, 1),
        ('{DoctorCredentials}', 'Kredensial Dokter', 'Doctor', 0, 1),
        ('{DoctorSpecialty}', 'Spesialisasi Dokter', 'Doctor', 0, 1),
     ('{TechnicianName}', 'Nama Teknisi', 'Doctor', 0, 1),
        ('{OperatorName}', 'Nama Operator', 'Doctor', 0, 1),
        -- DateTime
 ('{CurrentDate}', 'Tanggal Sekarang', 'DateTime', 3, 1),
 ('{CurrentTime}', 'Waktu Sekarang', 'DateTime', 0, 1),
        ('{CurrentDateTime}', 'Tanggal & Waktu Sekarang', 'DateTime', 4, 1),
      ('{PrintDate}', 'Tanggal Cetak', 'DateTime', 3, 1),
   -- Document
        ('{PageNumber}', 'Nomor Halaman', 'Document', 1, 1),
        ('{TotalPages}', 'Total Halaman', 'Document', 1, 1),
        ('{DocumentId}', 'ID Dokumen', 'Document', 0, 1);
    ";
}
