using Microsoft.Data.Sqlite;
using ReportBuilder.Core.Models;

namespace ReportBuilder.Core.Storage;

/// <summary>
/// SQLite implementation of the template repository.
/// </summary>
public class SqliteTemplateRepository : ITemplateRepository
{
    private readonly string _connectionString;

    public SqliteTemplateRepository(string connectionString)
    {
    _connectionString = connectionString;
    }

    public async Task<ReportTemplate?> GetByIdAsync(int id)
    {
    await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var template = await GetTemplateBaseAsync(connection, "WHERE t.Id = @Id", new SqliteParameter("@Id", id));
      if (template == null) return null;

        await LoadTemplateRelationsAsync(connection, template);
        return template;
    }

    public async Task<ReportTemplate?> GetByNameAsync(string name)
    {
  await using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync();

        var template = await GetTemplateBaseAsync(connection, "WHERE t.Name = @Name", new SqliteParameter("@Name", name));
        if (template == null) return null;

        await LoadTemplateRelationsAsync(connection, template);
        return template;
    }

    public async Task<ReportTemplate?> GetDefaultAsync()
 {
        await using var connection = new SqliteConnection(_connectionString);
 await connection.OpenAsync();

        var template = await GetTemplateBaseAsync(connection, "WHERE t.IsDefault = 1");
  if (template == null) return null;

        await LoadTemplateRelationsAsync(connection, template);
      return template;
    }

    public async Task<IEnumerable<ReportTemplate>> GetAllAsync(bool includeInactive = false)
    {
  await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var whereClause = includeInactive ? "" : "WHERE t.IsActive = 1";
        var templates = await GetTemplatesBaseAsync(connection, whereClause);

        foreach (var template in templates)
        {
            await LoadTemplateRelationsAsync(connection, template);
}

        return templates;
    }

    public async Task<int> CreateAsync(ReportTemplate template)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
 {
template.CreatedAt = DateTime.UtcNow;
            template.UpdatedAt = DateTime.UtcNow;

    // Insert template
            var templateId = await InsertTemplateAsync(connection, template);
      template.Id = templateId;

 // Insert header section
   if (template.Header != null)
    {
        template.Header.TemplateId = templateId;
             await InsertHeaderSectionAsync(connection, template.Header);
      }

   // Insert content section
   template.Content.TemplateId = templateId;
   await InsertContentSectionAsync(connection, template.Content);

        // Insert footer section
          if (template.Footer != null)
     {
       template.Footer.TemplateId = templateId;
     await InsertFooterSectionAsync(connection, template.Footer);
    }

            await transaction.CommitAsync();
      return templateId;
        }
        catch
        {
  await transaction.RollbackAsync();
     throw;
        }
    }

    public async Task<bool> UpdateAsync(ReportTemplate template)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
         template.UpdatedAt = DateTime.UtcNow;

       // Update template base
        await UpdateTemplateBaseAsync(connection, template);

            // Update header section
            if (template.Header != null)
          {
      await DeleteHeaderSectionAsync(connection, template.Id);
                template.Header.TemplateId = template.Id;
   await InsertHeaderSectionAsync(connection, template.Header);
      }

  // Update content section
await DeleteContentSectionAsync(connection, template.Id);
   template.Content.TemplateId = template.Id;
            await InsertContentSectionAsync(connection, template.Content);

   // Update footer section
            if (template.Footer != null)
        {
    await DeleteFooterSectionAsync(connection, template.Id);
 template.Footer.TemplateId = template.Id;
   await InsertFooterSectionAsync(connection, template.Footer);
       }

            await transaction.CommitAsync();
            return true;
        }
   catch
        {
            await transaction.RollbackAsync();
     throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
    await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

      const string sql = "DELETE FROM Templates WHERE Id = @Id";
        await using var command = new SqliteCommand(sql, connection);
   command.Parameters.AddWithValue("@Id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> SetDefaultAsync(int id)
    {
   await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
          // Clear existing default
     const string clearSql = "UPDATE Templates SET IsDefault = 0";
  await using (var clearCmd = new SqliteCommand(clearSql, connection))
   {
                await clearCmd.ExecuteNonQueryAsync();
      }

      // Set new default
          const string setSql = "UPDATE Templates SET IsDefault = 1 WHERE Id = @Id";
            await using (var setCmd = new SqliteCommand(setSql, connection))
        {
   setCmd.Parameters.AddWithValue("@Id", id);
       var rows = await setCmd.ExecuteNonQueryAsync();
           if (rows == 0)
           {
  await transaction.RollbackAsync();
                return false;
        }
 }

            await transaction.CommitAsync();
 return true;
        }
        catch
        {
            await transaction.RollbackAsync();
        throw;
        }
    }

    public async Task<bool> ExistsAsync(string name)
    {
    await using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync();

        const string sql = "SELECT COUNT(1) FROM Templates WHERE Name = @Name";
 await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Name", name);

  var count = Convert.ToInt32(await command.ExecuteScalarAsync());
   return count > 0;
    }

    public async Task<int> DuplicateAsync(int id, string newName)
    {
        var original = await GetByIdAsync(id);
        if (original == null)
            throw new InvalidOperationException($"Template with ID {id} not found");

        original.Id = 0;
        original.Name = newName;
   original.IsDefault = false;
        original.CreatedAt = DateTime.UtcNow;
        original.UpdatedAt = DateTime.UtcNow;

        return await CreateAsync(original);
 }

    #region Private Helper Methods

    private async Task<ReportTemplate?> GetTemplateBaseAsync(SqliteConnection connection, string whereClause, params SqliteParameter[] parameters)
    {
        var sql = $@"
    SELECT t.Id, t.Name, t.Description, t.PaperSize, t.Orientation,
            t.MarginTop, t.MarginBottom, t.MarginLeft, t.MarginRight,
          t.IsDefault, t.IsActive, t.CreatedAt, t.UpdatedAt
        FROM Templates t
  {whereClause}
        LIMIT 1";

        await using var command = new SqliteCommand(sql, connection);
        foreach (var param in parameters)
            command.Parameters.Add(param);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return MapReaderToTemplate(reader);
    }

    private async Task<List<ReportTemplate>> GetTemplatesBaseAsync(SqliteConnection connection, string whereClause)
    {
        var sql = $@"
      SELECT t.Id, t.Name, t.Description, t.PaperSize, t.Orientation,
t.MarginTop, t.MarginBottom, t.MarginLeft, t.MarginRight,
         t.IsDefault, t.IsActive, t.CreatedAt, t.UpdatedAt
        FROM Templates t
   {whereClause}
       ORDER BY t.Name";

        var templates = new List<ReportTemplate>();
      await using var command = new SqliteCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
     {
      templates.Add(MapReaderToTemplate(reader));
        }

        return templates;
    }

    private static ReportTemplate MapReaderToTemplate(SqliteDataReader reader)
    {
        return new ReportTemplate
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
      Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
PaperSize = (PaperSize)reader.GetInt32(3),
         Orientation = (PageOrientation)reader.GetInt32(4),
  Margins = new PageMargins
            {
     Top = reader.GetFloat(5),
       Bottom = reader.GetFloat(6),
   Left = reader.GetFloat(7),
         Right = reader.GetFloat(8)
        },
   IsDefault = reader.GetInt32(9) == 1,
        IsActive = reader.GetInt32(10) == 1,
            CreatedAt = DateTime.Parse(reader.GetString(11)),
            UpdatedAt = DateTime.Parse(reader.GetString(12))
 };
    }

    private async Task LoadTemplateRelationsAsync(SqliteConnection connection, ReportTemplate template)
    {
        template.Header = await LoadHeaderSectionAsync(connection, template.Id);
        template.Content = await LoadContentSectionAsync(connection, template.Id) ?? new ContentSection();
        template.Footer = await LoadFooterSectionAsync(connection, template.Id);
    }

    private async Task<int> InsertTemplateAsync(SqliteConnection connection, ReportTemplate template)
    {
        const string sql = @"
     INSERT INTO Templates (Name, Description, PaperSize, Orientation, 
              MarginTop, MarginBottom, MarginLeft, MarginRight, IsDefault, IsActive, CreatedAt, UpdatedAt)
         VALUES (@Name, @Description, @PaperSize, @Orientation,
       @MarginTop, @MarginBottom, @MarginLeft, @MarginRight, @IsDefault, @IsActive, @CreatedAt, @UpdatedAt);
            SELECT last_insert_rowid();";

        await using var command = new SqliteCommand(sql, connection);
command.Parameters.AddWithValue("@Name", template.Name);
   command.Parameters.AddWithValue("@Description", template.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@PaperSize", (int)template.PaperSize);
        command.Parameters.AddWithValue("@Orientation", (int)template.Orientation);
        command.Parameters.AddWithValue("@MarginTop", template.Margins.Top);
      command.Parameters.AddWithValue("@MarginBottom", template.Margins.Bottom);
        command.Parameters.AddWithValue("@MarginLeft", template.Margins.Left);
        command.Parameters.AddWithValue("@MarginRight", template.Margins.Right);
     command.Parameters.AddWithValue("@IsDefault", template.IsDefault ? 1 : 0);
   command.Parameters.AddWithValue("@IsActive", template.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("@CreatedAt", template.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("@UpdatedAt", template.UpdatedAt.ToString("O"));

    return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private async Task UpdateTemplateBaseAsync(SqliteConnection connection, ReportTemplate template)
    {
        const string sql = @"
         UPDATE Templates SET 
     Name = @Name, Description = @Description, PaperSize = @PaperSize, Orientation = @Orientation,
         MarginTop = @MarginTop, MarginBottom = @MarginBottom, MarginLeft = @MarginLeft, MarginRight = @MarginRight,
           IsDefault = @IsDefault, IsActive = @IsActive, UpdatedAt = @UpdatedAt
         WHERE Id = @Id";

      await using var command = new SqliteCommand(sql, connection);
    command.Parameters.AddWithValue("@Id", template.Id);
        command.Parameters.AddWithValue("@Name", template.Name);
        command.Parameters.AddWithValue("@Description", template.Description ?? (object)DBNull.Value);
      command.Parameters.AddWithValue("@PaperSize", (int)template.PaperSize);
        command.Parameters.AddWithValue("@Orientation", (int)template.Orientation);
    command.Parameters.AddWithValue("@MarginTop", template.Margins.Top);
    command.Parameters.AddWithValue("@MarginBottom", template.Margins.Bottom);
 command.Parameters.AddWithValue("@MarginLeft", template.Margins.Left);
        command.Parameters.AddWithValue("@MarginRight", template.Margins.Right);
        command.Parameters.AddWithValue("@IsDefault", template.IsDefault ? 1 : 0);
        command.Parameters.AddWithValue("@IsActive", template.IsActive ? 1 : 0);
 command.Parameters.AddWithValue("@UpdatedAt", template.UpdatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    // Header section methods
    private async Task<HeaderSection?> LoadHeaderSectionAsync(SqliteConnection connection, int templateId)
    {
    const string sql = @"
          SELECT Id, IsVisible, Height, LeftLogoVisible, LeftLogoPath, LeftLogoData, 
       LeftLogoWidth, LeftLogoHeight, LeftLogoVerticalAlign,
     RightLogoVisible, RightLogoPath, RightLogoData, 
  RightLogoWidth, RightLogoHeight, RightLogoVerticalAlign,
    ShowBorderBottom, BorderColor, BorderThickness, BackgroundColor
   FROM HeaderSections WHERE TemplateId = @TemplateId";

        await using var command = new SqliteCommand(sql, connection);
    command.Parameters.AddWithValue("@TemplateId", templateId);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        var header = new HeaderSection
        {
            Id = reader.GetInt32(0),
            TemplateId = templateId,
      IsVisible = reader.GetInt32(1) == 1,
            Height = reader.GetFloat(2),
            LeftLogo = new LogoPlacement
          {
      IsVisible = reader.GetInt32(3) == 1,
    ImagePath = reader.IsDBNull(4) ? null : reader.GetString(4),
         ImageData = reader.IsDBNull(5) ? null : (byte[])reader.GetValue(5),
           Width = reader.GetFloat(6),
        Height = reader.GetFloat(7),
        VerticalAlignment = (VerticalAlignment)reader.GetInt32(8)
    },
            RightLogo = new LogoPlacement
        {
        IsVisible = reader.GetInt32(9) == 1,
        ImagePath = reader.IsDBNull(10) ? null : reader.GetString(10),
      ImageData = reader.IsDBNull(11) ? null : (byte[])reader.GetValue(11),
   Width = reader.GetFloat(12),
         Height = reader.GetFloat(13),
      VerticalAlignment = (VerticalAlignment)reader.GetInt32(14)
            },
     ShowBorderBottom = reader.GetInt32(15) == 1,
  BorderColor = reader.GetString(16),
  BorderThickness = reader.GetFloat(17),
  BackgroundColor = reader.IsDBNull(18) ? null : reader.GetString(18)
        };

        // Load header lines
        header.Lines = await LoadHeaderLinesAsync(connection, header.Id);

        return header;
    }

    private async Task<List<HeaderLine>> LoadHeaderLinesAsync(SqliteConnection connection, int headerSectionId)
    {
        const string sql = @"
            SELECT Id, LineOrder, Text, PlaceholderKey, FontSize, IsBold, IsItalic, IsUnderline,
   FontFamily, FontColor, Alignment, IsVisible, MarginTop, MarginBottom
         FROM HeaderLines WHERE HeaderSectionId = @HeaderSectionId ORDER BY LineOrder";

   var lines = new List<HeaderLine>();
        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@HeaderSectionId", headerSectionId);

        await using var reader = await command.ExecuteReaderAsync();
while (await reader.ReadAsync())
        {
            lines.Add(new HeaderLine
  {
     Id = reader.GetInt32(0),
             HeaderSectionId = headerSectionId,
       Order = reader.GetInt32(1),
         Text = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
 PlaceholderKey = reader.IsDBNull(3) ? null : reader.GetString(3),
           FontSize = reader.GetFloat(4),
                IsBold = reader.GetInt32(5) == 1,
     IsItalic = reader.GetInt32(6) == 1,
    IsUnderline = reader.GetInt32(7) == 1,
     FontFamily = reader.GetString(8),
                FontColor = reader.GetString(9),
  Alignment = (TextAlignment)reader.GetInt32(10),
         IsVisible = reader.GetInt32(11) == 1,
                MarginTop = reader.GetFloat(12),
        MarginBottom = reader.GetFloat(13)
     });
        }

        return lines;
    }

    private async Task InsertHeaderSectionAsync(SqliteConnection connection, HeaderSection header)
    {
        const string sql = @"
     INSERT INTO HeaderSections (TemplateId, IsVisible, Height,
             LeftLogoVisible, LeftLogoPath, LeftLogoData, LeftLogoWidth, LeftLogoHeight, LeftLogoVerticalAlign,
             RightLogoVisible, RightLogoPath, RightLogoData, RightLogoWidth, RightLogoHeight, RightLogoVerticalAlign,
                ShowBorderBottom, BorderColor, BorderThickness, BackgroundColor)
   VALUES (@TemplateId, @IsVisible, @Height,
    @LeftLogoVisible, @LeftLogoPath, @LeftLogoData, @LeftLogoWidth, @LeftLogoHeight, @LeftLogoVerticalAlign,
   @RightLogoVisible, @RightLogoPath, @RightLogoData, @RightLogoWidth, @RightLogoHeight, @RightLogoVerticalAlign,
           @ShowBorderBottom, @BorderColor, @BorderThickness, @BackgroundColor);
            SELECT last_insert_rowid();";

        await using var command = new SqliteCommand(sql, connection);
 command.Parameters.AddWithValue("@TemplateId", header.TemplateId);
        command.Parameters.AddWithValue("@IsVisible", header.IsVisible ? 1 : 0);
        command.Parameters.AddWithValue("@Height", header.Height);
  command.Parameters.AddWithValue("@LeftLogoVisible", header.LeftLogo.IsVisible ? 1 : 0);
    command.Parameters.AddWithValue("@LeftLogoPath", header.LeftLogo.ImagePath ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@LeftLogoData", header.LeftLogo.ImageData ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@LeftLogoWidth", header.LeftLogo.Width);
   command.Parameters.AddWithValue("@LeftLogoHeight", header.LeftLogo.Height);
        command.Parameters.AddWithValue("@LeftLogoVerticalAlign", (int)header.LeftLogo.VerticalAlignment);
        command.Parameters.AddWithValue("@RightLogoVisible", header.RightLogo.IsVisible ? 1 : 0);
        command.Parameters.AddWithValue("@RightLogoPath", header.RightLogo.ImagePath ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@RightLogoData", header.RightLogo.ImageData ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@RightLogoWidth", header.RightLogo.Width);
        command.Parameters.AddWithValue("@RightLogoHeight", header.RightLogo.Height);
        command.Parameters.AddWithValue("@RightLogoVerticalAlign", (int)header.RightLogo.VerticalAlignment);
        command.Parameters.AddWithValue("@ShowBorderBottom", header.ShowBorderBottom ? 1 : 0);
        command.Parameters.AddWithValue("@BorderColor", header.BorderColor);
      command.Parameters.AddWithValue("@BorderThickness", header.BorderThickness);
        command.Parameters.AddWithValue("@BackgroundColor", header.BackgroundColor ?? (object)DBNull.Value);

        var headerId = Convert.ToInt32(await command.ExecuteScalarAsync());
     header.Id = headerId;

 // Insert header lines
     foreach (var line in header.Lines)
        {
  line.HeaderSectionId = headerId;
     await InsertHeaderLineAsync(connection, line);
 }
    }

    private async Task InsertHeaderLineAsync(SqliteConnection connection, HeaderLine line)
    {
  const string sql = @"
         INSERT INTO HeaderLines (HeaderSectionId, LineOrder, Text, PlaceholderKey, FontSize, 
  IsBold, IsItalic, IsUnderline, FontFamily, FontColor, Alignment, IsVisible, MarginTop, MarginBottom)
        VALUES (@HeaderSectionId, @LineOrder, @Text, @PlaceholderKey, @FontSize,
   @IsBold, @IsItalic, @IsUnderline, @FontFamily, @FontColor, @Alignment, @IsVisible, @MarginTop, @MarginBottom)";

        await using var command = new SqliteCommand(sql, connection);
      command.Parameters.AddWithValue("@HeaderSectionId", line.HeaderSectionId);
        command.Parameters.AddWithValue("@LineOrder", line.Order);
        command.Parameters.AddWithValue("@Text", line.Text ?? (object)DBNull.Value);
command.Parameters.AddWithValue("@PlaceholderKey", line.PlaceholderKey ?? (object)DBNull.Value);
  command.Parameters.AddWithValue("@FontSize", line.FontSize);
        command.Parameters.AddWithValue("@IsBold", line.IsBold ? 1 : 0);
        command.Parameters.AddWithValue("@IsItalic", line.IsItalic ? 1 : 0);
        command.Parameters.AddWithValue("@IsUnderline", line.IsUnderline ? 1 : 0);
   command.Parameters.AddWithValue("@FontFamily", line.FontFamily);
        command.Parameters.AddWithValue("@FontColor", line.FontColor);
        command.Parameters.AddWithValue("@Alignment", (int)line.Alignment);
        command.Parameters.AddWithValue("@IsVisible", line.IsVisible ? 1 : 0);
    command.Parameters.AddWithValue("@MarginTop", line.MarginTop);
        command.Parameters.AddWithValue("@MarginBottom", line.MarginBottom);

    await command.ExecuteNonQueryAsync();
    }

    private async Task DeleteHeaderSectionAsync(SqliteConnection connection, int templateId)
    {
        const string sql = "DELETE FROM HeaderSections WHERE TemplateId = @TemplateId";
        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@TemplateId", templateId);
        await command.ExecuteNonQueryAsync();
    }

    // Content section methods
    private async Task<ContentSection?> LoadContentSectionAsync(SqliteConnection connection, int templateId)
    {
    const string sql = "SELECT Id FROM ContentSections WHERE TemplateId = @TemplateId";
await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@TemplateId", templateId);

        var contentId = await command.ExecuteScalarAsync();
        if (contentId == null) return null;

        var content = new ContentSection
        {
            Id = Convert.ToInt32(contentId),
     TemplateId = templateId
        };

        content.InfoFields = await LoadInfoFieldsLayoutAsync(connection, content.Id) ?? new InfoFieldsLayout();
        content.ImageGrid = await LoadImageGridLayoutAsync(connection, content.Id) ?? new ImageGridLayout();
        content.Results = await LoadResultSectionAsync(connection, content.Id) ?? new ResultSection();
    content.CustomSections = await LoadCustomSectionsAsync(connection, content.Id);

     return content;
    }

    private async Task<InfoFieldsLayout?> LoadInfoFieldsLayoutAsync(SqliteConnection connection, int contentSectionId)
    {
        const string sql = @"
          SELECT Id, IsVisible, ColumnsCount, FontSize, FontFamily, RowSpacing, 
    ColumnSpacing, ShowBorder, BorderColor, Padding
     FROM InfoFieldsLayouts WHERE ContentSectionId = @ContentSectionId";

        await using var command = new SqliteCommand(sql, connection);
      command.Parameters.AddWithValue("@ContentSectionId", contentSectionId);

        await using var reader = await command.ExecuteReaderAsync();
    if (!await reader.ReadAsync()) return null;

        var layout = new InfoFieldsLayout
        {
     Id = reader.GetInt32(0),
  IsVisible = reader.GetInt32(1) == 1,
    ColumnsCount = reader.GetInt32(2),
        FontSize = reader.GetFloat(3),
            FontFamily = reader.GetString(4),
  RowSpacing = reader.GetFloat(5),
     ColumnSpacing = reader.GetFloat(6),
       ShowBorder = reader.GetInt32(7) == 1,
   BorderColor = reader.GetString(8),
   Padding = reader.GetFloat(9)
        };

    layout.Fields = await LoadInfoFieldsAsync(connection, layout.Id);
        return layout;
    }

    private async Task<List<InfoField>> LoadInfoFieldsAsync(SqliteConnection connection, int layoutId)
    {
        const string sql = @"
    SELECT Id, Label, PlaceholderKey, ColumnIndex, FieldOrder, IsVisible, 
         LabelBold, ValueBold, Separator, LabelWidth
        FROM InfoFields WHERE InfoFieldsLayoutId = @LayoutId ORDER BY FieldOrder";

        var fields = new List<InfoField>();
        await using var command = new SqliteCommand(sql, connection);
      command.Parameters.AddWithValue("@LayoutId", layoutId);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
     {
    fields.Add(new InfoField
      {
                Id = reader.GetInt32(0),
       Label = reader.GetString(1),
                PlaceholderKey = reader.GetString(2),
        Column = reader.GetInt32(3),
    Order = reader.GetInt32(4),
  IsVisible = reader.GetInt32(5) == 1,
LabelBold = reader.GetInt32(6) == 1,
        ValueBold = reader.GetInt32(7) == 1,
           Separator = reader.GetString(8),
        LabelWidth = reader.IsDBNull(9) ? null : reader.GetFloat(9)
   });
        }

    return fields;
    }

    private async Task<ImageGridLayout?> LoadImageGridLayoutAsync(SqliteConnection connection, int contentSectionId)
    {
    const string sql = @"
SELECT Id, IsVisible, Columns, Rows, MaxImages, ImageSpacing, ShowImageBorder,
          BorderColor, BorderThickness, ScaleMode, FixedWidth, FixedHeight, 
        ShowImageNumbers, BackgroundColor, CornerRadius
        FROM ImageGridLayouts WHERE ContentSectionId = @ContentSectionId";

        await using var command = new SqliteCommand(sql, connection);
      command.Parameters.AddWithValue("@ContentSectionId", contentSectionId);

  await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

 return new ImageGridLayout
        {
            Id = reader.GetInt32(0),
     IsVisible = reader.GetInt32(1) == 1,
    Columns = reader.GetInt32(2),
       Rows = reader.GetInt32(3),
          MaxImages = reader.GetInt32(4),
      ImageSpacing = reader.GetFloat(5),
   ShowImageBorder = reader.GetInt32(6) == 1,
         BorderColor = reader.GetString(7),
            BorderThickness = reader.GetFloat(8),
            ScaleMode = (ImageScaleMode)reader.GetInt32(9),
   FixedWidth = reader.IsDBNull(10) ? null : reader.GetFloat(10),
          FixedHeight = reader.IsDBNull(11) ? null : reader.GetFloat(11),
   ShowImageNumbers = reader.GetInt32(12) == 1,
  BackgroundColor = reader.IsDBNull(13) ? null : reader.GetString(13),
         CornerRadius = reader.GetFloat(14)
  };
    }

    private async Task<ResultSection?> LoadResultSectionAsync(SqliteConnection connection, int contentSectionId)
    {
        const string sql = @"
    SELECT Id, IsVisible, Label, PlaceholderKey, FontSize, FontFamily,
  LabelBold, ValueBold, MinHeight, ShowBorder, BorderColor, Padding
       FROM ResultSections WHERE ContentSectionId = @ContentSectionId";

        await using var command = new SqliteCommand(sql, connection);
   command.Parameters.AddWithValue("@ContentSectionId", contentSectionId);

 await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

   return new ResultSection
        {
      Id = reader.GetInt32(0),
            IsVisible = reader.GetInt32(1) == 1,
            Label = reader.GetString(2),
      PlaceholderKey = reader.GetString(3),
   FontSize = reader.GetFloat(4),
            FontFamily = reader.GetString(5),
          LabelBold = reader.GetInt32(6) == 1,
  ValueBold = reader.GetInt32(7) == 1,
            MinHeight = reader.GetFloat(8),
          ShowBorder = reader.GetInt32(9) == 1,
 BorderColor = reader.GetString(10),
         Padding = reader.GetFloat(11)
        };
    }

    private async Task<List<CustomSection>> LoadCustomSectionsAsync(SqliteConnection connection, int contentSectionId)
    {
        const string sql = @"
   SELECT Id, Name, SectionOrder, IsVisible, Type, Content, PlaceholderKey,
   FontSize, FontFamily, IsBold, Alignment, MarginTop, MarginBottom
            FROM CustomSections WHERE ContentSectionId = @ContentSectionId ORDER BY SectionOrder";

        var sections = new List<CustomSection>();
        await using var command = new SqliteCommand(sql, connection);
   command.Parameters.AddWithValue("@ContentSectionId", contentSectionId);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
sections.Add(new CustomSection
            {
    Id = reader.GetInt32(0),
         Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
         Order = reader.GetInt32(2),
         IsVisible = reader.GetInt32(3) == 1,
         Type = (CustomSectionType)reader.GetInt32(4),
         Content = reader.IsDBNull(5) ? null : reader.GetString(5),
          PlaceholderKey = reader.IsDBNull(6) ? null : reader.GetString(6),
          FontSize = reader.GetFloat(7),
            FontFamily = reader.GetString(8),
   IsBold = reader.GetInt32(9) == 1,
  Alignment = (TextAlignment)reader.GetInt32(10),
      MarginTop = reader.GetFloat(11),
           MarginBottom = reader.GetFloat(12)
            });
   }

      return sections;
    }

    private async Task InsertContentSectionAsync(SqliteConnection connection, ContentSection content)
    {
        const string sql = @"
        INSERT INTO ContentSections (TemplateId) VALUES (@TemplateId);
          SELECT last_insert_rowid();";

        await using var command = new SqliteCommand(sql, connection);
      command.Parameters.AddWithValue("@TemplateId", content.TemplateId);

      var contentId = Convert.ToInt32(await command.ExecuteScalarAsync());
        content.Id = contentId;

        // Insert info fields layout
        await InsertInfoFieldsLayoutAsync(connection, content.InfoFields, contentId);

        // Insert image grid layout
  await InsertImageGridLayoutAsync(connection, content.ImageGrid, contentId);

     // Insert result section
        await InsertResultSectionAsync(connection, content.Results, contentId);

        // Insert custom sections
        foreach (var section in content.CustomSections)
   {
            await InsertCustomSectionAsync(connection, section, contentId);
    }
    }

    private async Task InsertInfoFieldsLayoutAsync(SqliteConnection connection, InfoFieldsLayout layout, int contentSectionId)
    {
 const string sql = @"
INSERT INTO InfoFieldsLayouts (ContentSectionId, IsVisible, ColumnsCount, FontSize, 
    FontFamily, RowSpacing, ColumnSpacing, ShowBorder, BorderColor, Padding)
       VALUES (@ContentSectionId, @IsVisible, @ColumnsCount, @FontSize,
             @FontFamily, @RowSpacing, @ColumnSpacing, @ShowBorder, @BorderColor, @Padding);
    SELECT last_insert_rowid();";

    await using var command = new SqliteCommand(sql, connection);
 command.Parameters.AddWithValue("@ContentSectionId", contentSectionId);
        command.Parameters.AddWithValue("@IsVisible", layout.IsVisible ? 1 : 0);
        command.Parameters.AddWithValue("@ColumnsCount", layout.ColumnsCount);
        command.Parameters.AddWithValue("@FontSize", layout.FontSize);
    command.Parameters.AddWithValue("@FontFamily", layout.FontFamily);
        command.Parameters.AddWithValue("@RowSpacing", layout.RowSpacing);
        command.Parameters.AddWithValue("@ColumnSpacing", layout.ColumnSpacing);
        command.Parameters.AddWithValue("@ShowBorder", layout.ShowBorder ? 1 : 0);
        command.Parameters.AddWithValue("@BorderColor", layout.BorderColor);
        command.Parameters.AddWithValue("@Padding", layout.Padding);

   var layoutId = Convert.ToInt32(await command.ExecuteScalarAsync());
        layout.Id = layoutId;

        foreach (var field in layout.Fields)
        {
   await InsertInfoFieldAsync(connection, field, layoutId);
 }
    }

    private async Task InsertInfoFieldAsync(SqliteConnection connection, InfoField field, int layoutId)
    {
        const string sql = @"
            INSERT INTO InfoFields (InfoFieldsLayoutId, Label, PlaceholderKey, ColumnIndex, 
     FieldOrder, IsVisible, LabelBold, ValueBold, Separator, LabelWidth)
    VALUES (@LayoutId, @Label, @PlaceholderKey, @ColumnIndex,
       @FieldOrder, @IsVisible, @LabelBold, @ValueBold, @Separator, @LabelWidth)";

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@LayoutId", layoutId);
        command.Parameters.AddWithValue("@Label", field.Label);
    command.Parameters.AddWithValue("@PlaceholderKey", field.PlaceholderKey);
command.Parameters.AddWithValue("@ColumnIndex", field.Column);
      command.Parameters.AddWithValue("@FieldOrder", field.Order);
 command.Parameters.AddWithValue("@IsVisible", field.IsVisible ? 1 : 0);
     command.Parameters.AddWithValue("@LabelBold", field.LabelBold ? 1 : 0);
        command.Parameters.AddWithValue("@ValueBold", field.ValueBold ? 1 : 0);
        command.Parameters.AddWithValue("@Separator", field.Separator);
        command.Parameters.AddWithValue("@LabelWidth", field.LabelWidth ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    private async Task InsertImageGridLayoutAsync(SqliteConnection connection, ImageGridLayout grid, int contentSectionId)
    {
        const string sql = @"
       INSERT INTO ImageGridLayouts (ContentSectionId, IsVisible, Columns, Rows, MaxImages,
                ImageSpacing, ShowImageBorder, BorderColor, BorderThickness, ScaleMode,
    FixedWidth, FixedHeight, ShowImageNumbers, BackgroundColor, CornerRadius)
            VALUES (@ContentSectionId, @IsVisible, @Columns, @Rows, @MaxImages,
     @ImageSpacing, @ShowImageBorder, @BorderColor, @BorderThickness, @ScaleMode,
    @FixedWidth, @FixedHeight, @ShowImageNumbers, @BackgroundColor, @CornerRadius)";

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@ContentSectionId", contentSectionId);
     command.Parameters.AddWithValue("@IsVisible", grid.IsVisible ? 1 : 0);
        command.Parameters.AddWithValue("@Columns", grid.Columns);
        command.Parameters.AddWithValue("@Rows", grid.Rows);
        command.Parameters.AddWithValue("@MaxImages", grid.MaxImages);
        command.Parameters.AddWithValue("@ImageSpacing", grid.ImageSpacing);
        command.Parameters.AddWithValue("@ShowImageBorder", grid.ShowImageBorder ? 1 : 0);
      command.Parameters.AddWithValue("@BorderColor", grid.BorderColor);
        command.Parameters.AddWithValue("@BorderThickness", grid.BorderThickness);
        command.Parameters.AddWithValue("@ScaleMode", (int)grid.ScaleMode);
        command.Parameters.AddWithValue("@FixedWidth", grid.FixedWidth ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@FixedHeight", grid.FixedHeight ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ShowImageNumbers", grid.ShowImageNumbers ? 1 : 0);
        command.Parameters.AddWithValue("@BackgroundColor", grid.BackgroundColor ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CornerRadius", grid.CornerRadius);

        await command.ExecuteNonQueryAsync();
    }

    private async Task InsertResultSectionAsync(SqliteConnection connection, ResultSection result, int contentSectionId)
    {
        const string sql = @"
            INSERT INTO ResultSections (ContentSectionId, IsVisible, Label, PlaceholderKey, 
      FontSize, FontFamily, LabelBold, ValueBold, MinHeight, ShowBorder, BorderColor, Padding)
          VALUES (@ContentSectionId, @IsVisible, @Label, @PlaceholderKey,
      @FontSize, @FontFamily, @LabelBold, @ValueBold, @MinHeight, @ShowBorder, @BorderColor, @Padding)";

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@ContentSectionId", contentSectionId);
        command.Parameters.AddWithValue("@IsVisible", result.IsVisible ? 1 : 0);
        command.Parameters.AddWithValue("@Label", result.Label);
   command.Parameters.AddWithValue("@PlaceholderKey", result.PlaceholderKey);
        command.Parameters.AddWithValue("@FontSize", result.FontSize);
        command.Parameters.AddWithValue("@FontFamily", result.FontFamily);
        command.Parameters.AddWithValue("@LabelBold", result.LabelBold ? 1 : 0);
   command.Parameters.AddWithValue("@ValueBold", result.ValueBold ? 1 : 0);
   command.Parameters.AddWithValue("@MinHeight", result.MinHeight);
 command.Parameters.AddWithValue("@ShowBorder", result.ShowBorder ? 1 : 0);
        command.Parameters.AddWithValue("@BorderColor", result.BorderColor);
        command.Parameters.AddWithValue("@Padding", result.Padding);

        await command.ExecuteNonQueryAsync();
    }

    private async Task InsertCustomSectionAsync(SqliteConnection connection, CustomSection section, int contentSectionId)
    {
     const string sql = @"
   INSERT INTO CustomSections (ContentSectionId, Name, SectionOrder, IsVisible, Type,
Content, PlaceholderKey, FontSize, FontFamily, IsBold, Alignment, MarginTop, MarginBottom)
      VALUES (@ContentSectionId, @Name, @SectionOrder, @IsVisible, @Type,
         @Content, @PlaceholderKey, @FontSize, @FontFamily, @IsBold, @Alignment, @MarginTop, @MarginBottom)";

     await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@ContentSectionId", contentSectionId);
   command.Parameters.AddWithValue("@Name", section.Name ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@SectionOrder", section.Order);
        command.Parameters.AddWithValue("@IsVisible", section.IsVisible ? 1 : 0);
        command.Parameters.AddWithValue("@Type", (int)section.Type);
        command.Parameters.AddWithValue("@Content", section.Content ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@PlaceholderKey", section.PlaceholderKey ?? (object)DBNull.Value);
   command.Parameters.AddWithValue("@FontSize", section.FontSize);
        command.Parameters.AddWithValue("@FontFamily", section.FontFamily);
        command.Parameters.AddWithValue("@IsBold", section.IsBold ? 1 : 0);
  command.Parameters.AddWithValue("@Alignment", (int)section.Alignment);
        command.Parameters.AddWithValue("@MarginTop", section.MarginTop);
        command.Parameters.AddWithValue("@MarginBottom", section.MarginBottom);

     await command.ExecuteNonQueryAsync();
    }

    private async Task DeleteContentSectionAsync(SqliteConnection connection, int templateId)
    {
        const string sql = "DELETE FROM ContentSections WHERE TemplateId = @TemplateId";
        await using var command = new SqliteCommand(sql, connection);
  command.Parameters.AddWithValue("@TemplateId", templateId);
        await command.ExecuteNonQueryAsync();
    }

    // Footer section methods
    private async Task<FooterSection?> LoadFooterSectionAsync(SqliteConnection connection, int templateId)
    {
        const string sql = @"
      SELECT Id, IsVisible, Height, ShowBorderTop, BorderColor, BorderThickness, BackgroundColor
 FROM FooterSections WHERE TemplateId = @TemplateId";

    await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@TemplateId", templateId);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        var footer = new FooterSection
        {
Id = reader.GetInt32(0),
  TemplateId = templateId,
            IsVisible = reader.GetInt32(1) == 1,
            Height = reader.GetFloat(2),
            ShowBorderTop = reader.GetInt32(3) == 1,
   BorderColor = reader.GetString(4),
       BorderThickness = reader.GetFloat(5),
      BackgroundColor = reader.IsDBNull(6) ? null : reader.GetString(6)
        };

        footer.Signature = await LoadSignatureBlockAsync(connection, footer.Id) ?? new SignatureBlock();
        footer.DateLocation = await LoadDateLocationBlockAsync(connection, footer.Id) ?? new DateLocationBlock();
        footer.AdditionalElements = await LoadFooterElementsAsync(connection, footer.Id);

        return footer;
    }

    private async Task<SignatureBlock?> LoadSignatureBlockAsync(SqliteConnection connection, int footerSectionId)
    {
        const string sql = @"
         SELECT Id, IsVisible, TitleLabel, NamePlaceholder, CredentialsPlaceholder,
                   SignatureSpaceHeight, Position, FontSize, FontFamily, ShowSignatureLine, SignatureLineWidth
   FROM SignatureBlocks WHERE FooterSectionId = @FooterSectionId";

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@FooterSectionId", footerSectionId);

        await using var reader = await command.ExecuteReaderAsync();
    if (!await reader.ReadAsync()) return null;

        return new SignatureBlock
        {
            Id = reader.GetInt32(0),
            IsVisible = reader.GetInt32(1) == 1,
       TitleLabel = reader.GetString(2),
      NamePlaceholder = reader.GetString(3),
     CredentialsPlaceholder = reader.IsDBNull(4) ? null : reader.GetString(4),
            SignatureSpaceHeight = reader.GetFloat(5),
    Position = (HorizontalPosition)reader.GetInt32(6),
FontSize = reader.GetFloat(7),
       FontFamily = reader.GetString(8),
            ShowSignatureLine = reader.GetInt32(9) == 1,
            SignatureLineWidth = reader.GetFloat(10)
        };
    }

    private async Task<DateLocationBlock?> LoadDateLocationBlockAsync(SqliteConnection connection, int footerSectionId)
    {
  const string sql = @"
            SELECT Id, IsVisible, CityName, DateFormat, CultureCode, Position, FontSize, FontFamily, CustomText
            FROM DateLocationBlocks WHERE FooterSectionId = @FooterSectionId";

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@FooterSectionId", footerSectionId);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new DateLocationBlock
        {
     Id = reader.GetInt32(0),
       IsVisible = reader.GetInt32(1) == 1,
        CityName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
      DateFormat = reader.GetString(3),
       CultureCode = reader.GetString(4),
            Position = (HorizontalPosition)reader.GetInt32(5),
            FontSize = reader.GetFloat(6),
   FontFamily = reader.GetString(7),
            CustomText = reader.IsDBNull(8) ? null : reader.GetString(8)
        };
  }

    private async Task<List<FooterElement>> LoadFooterElementsAsync(SqliteConnection connection, int footerSectionId)
    {
        const string sql = @"
     SELECT Id, Name, ElementOrder, Type, Content, PlaceholderKey, Position,
     FontSize, FontFamily, FontColor, IsBold, IsItalic, IsVisible
      FROM FooterElements WHERE FooterSectionId = @FooterSectionId ORDER BY ElementOrder";

    var elements = new List<FooterElement>();
        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@FooterSectionId", footerSectionId);

   await using var reader = await command.ExecuteReaderAsync();
   while (await reader.ReadAsync())
        {
            elements.Add(new FooterElement
    {
        Id = reader.GetInt32(0),
      Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
        Order = reader.GetInt32(2),
          Type = (FooterElementType)reader.GetInt32(3),
         Content = reader.IsDBNull(4) ? null : reader.GetString(4),
            PlaceholderKey = reader.IsDBNull(5) ? null : reader.GetString(5),
                Position = (HorizontalPosition)reader.GetInt32(6),
      FontSize = reader.GetFloat(7),
        FontFamily = reader.GetString(8),
         FontColor = reader.GetString(9),
    IsBold = reader.GetInt32(10) == 1,
          IsItalic = reader.GetInt32(11) == 1,
    IsVisible = reader.GetInt32(12) == 1
 });
        }

        return elements;
    }

    private async Task InsertFooterSectionAsync(SqliteConnection connection, FooterSection footer)
    {
        const string sql = @"
   INSERT INTO FooterSections (TemplateId, IsVisible, Height, ShowBorderTop, 
         BorderColor, BorderThickness, BackgroundColor)
    VALUES (@TemplateId, @IsVisible, @Height, @ShowBorderTop,
@BorderColor, @BorderThickness, @BackgroundColor);
 SELECT last_insert_rowid();";

        await using var command = new SqliteCommand(sql, connection);
 command.Parameters.AddWithValue("@TemplateId", footer.TemplateId);
        command.Parameters.AddWithValue("@IsVisible", footer.IsVisible ? 1 : 0);
    command.Parameters.AddWithValue("@Height", footer.Height);
        command.Parameters.AddWithValue("@ShowBorderTop", footer.ShowBorderTop ? 1 : 0);
  command.Parameters.AddWithValue("@BorderColor", footer.BorderColor);
        command.Parameters.AddWithValue("@BorderThickness", footer.BorderThickness);
        command.Parameters.AddWithValue("@BackgroundColor", footer.BackgroundColor ?? (object)DBNull.Value);

        var footerId = Convert.ToInt32(await command.ExecuteScalarAsync());
        footer.Id = footerId;

        // Insert signature block
        await InsertSignatureBlockAsync(connection, footer.Signature, footerId);

  // Insert date location block
        await InsertDateLocationBlockAsync(connection, footer.DateLocation, footerId);

        // Insert additional elements
    foreach (var element in footer.AdditionalElements)
  {
     await InsertFooterElementAsync(connection, element, footerId);
        }
    }

    private async Task InsertSignatureBlockAsync(SqliteConnection connection, SignatureBlock signature, int footerSectionId)
    {
    const string sql = @"
       INSERT INTO SignatureBlocks (FooterSectionId, IsVisible, TitleLabel, NamePlaceholder,
    CredentialsPlaceholder, SignatureSpaceHeight, Position, FontSize, FontFamily, 
  ShowSignatureLine, SignatureLineWidth)
        VALUES (@FooterSectionId, @IsVisible, @TitleLabel, @NamePlaceholder,
        @CredentialsPlaceholder, @SignatureSpaceHeight, @Position, @FontSize, @FontFamily,
        @ShowSignatureLine, @SignatureLineWidth)";

     await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@FooterSectionId", footerSectionId);
      command.Parameters.AddWithValue("@IsVisible", signature.IsVisible ? 1 : 0);
command.Parameters.AddWithValue("@TitleLabel", signature.TitleLabel);
        command.Parameters.AddWithValue("@NamePlaceholder", signature.NamePlaceholder);
  command.Parameters.AddWithValue("@CredentialsPlaceholder", signature.CredentialsPlaceholder ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@SignatureSpaceHeight", signature.SignatureSpaceHeight);
      command.Parameters.AddWithValue("@Position", (int)signature.Position);
        command.Parameters.AddWithValue("@FontSize", signature.FontSize);
        command.Parameters.AddWithValue("@FontFamily", signature.FontFamily);
 command.Parameters.AddWithValue("@ShowSignatureLine", signature.ShowSignatureLine ? 1 : 0);
        command.Parameters.AddWithValue("@SignatureLineWidth", signature.SignatureLineWidth);

        await command.ExecuteNonQueryAsync();
    }

    private async Task InsertDateLocationBlockAsync(SqliteConnection connection, DateLocationBlock dateLocation, int footerSectionId)
    {
        const string sql = @"
  INSERT INTO DateLocationBlocks (FooterSectionId, IsVisible, CityName, DateFormat,
        CultureCode, Position, FontSize, FontFamily, CustomText)
   VALUES (@FooterSectionId, @IsVisible, @CityName, @DateFormat,
  @CultureCode, @Position, @FontSize, @FontFamily, @CustomText)";

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@FooterSectionId", footerSectionId);
     command.Parameters.AddWithValue("@IsVisible", dateLocation.IsVisible ? 1 : 0);
        command.Parameters.AddWithValue("@CityName", dateLocation.CityName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@DateFormat", dateLocation.DateFormat);
  command.Parameters.AddWithValue("@CultureCode", dateLocation.CultureCode);
   command.Parameters.AddWithValue("@Position", (int)dateLocation.Position);
   command.Parameters.AddWithValue("@FontSize", dateLocation.FontSize);
        command.Parameters.AddWithValue("@FontFamily", dateLocation.FontFamily);
        command.Parameters.AddWithValue("@CustomText", dateLocation.CustomText ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    private async Task InsertFooterElementAsync(SqliteConnection connection, FooterElement element, int footerSectionId)
{
    const string sql = @"
            INSERT INTO FooterElements (FooterSectionId, Name, ElementOrder, Type, Content,
     PlaceholderKey, Position, FontSize, FontFamily, FontColor, IsBold, IsItalic, IsVisible)
    VALUES (@FooterSectionId, @Name, @ElementOrder, @Type, @Content,
        @PlaceholderKey, @Position, @FontSize, @FontFamily, @FontColor, @IsBold, @IsItalic, @IsVisible)";

   await using var command = new SqliteCommand(sql, connection);
     command.Parameters.AddWithValue("@FooterSectionId", footerSectionId);
        command.Parameters.AddWithValue("@Name", element.Name ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ElementOrder", element.Order);
  command.Parameters.AddWithValue("@Type", (int)element.Type);
 command.Parameters.AddWithValue("@Content", element.Content ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@PlaceholderKey", element.PlaceholderKey ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Position", (int)element.Position);
        command.Parameters.AddWithValue("@FontSize", element.FontSize);
        command.Parameters.AddWithValue("@FontFamily", element.FontFamily);
        command.Parameters.AddWithValue("@FontColor", element.FontColor);
     command.Parameters.AddWithValue("@IsBold", element.IsBold ? 1 : 0);
        command.Parameters.AddWithValue("@IsItalic", element.IsItalic ? 1 : 0);
        command.Parameters.AddWithValue("@IsVisible", element.IsVisible ? 1 : 0);

      await command.ExecuteNonQueryAsync();
    }

    private async Task DeleteFooterSectionAsync(SqliteConnection connection, int templateId)
  {
        const string sql = "DELETE FROM FooterSections WHERE TemplateId = @TemplateId";
    await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@TemplateId", templateId);
        await command.ExecuteNonQueryAsync();
    }

    #endregion
}
