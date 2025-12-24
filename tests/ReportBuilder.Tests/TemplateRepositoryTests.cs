using ReportBuilder.Core.Models;
using ReportBuilder.Core.Storage;

namespace ReportBuilder.Tests;

public class TemplateRepositoryTests : IAsyncLifetime
{
    private readonly string _testDbPath;
  private SqliteUnitOfWork _unitOfWork = null!;

  public TemplateRepositoryTests()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"reportbuilder_test_{Guid.NewGuid()}.db");
}

    public async Task InitializeAsync()
  {
        _unitOfWork = new SqliteUnitOfWork(_testDbPath);
   await _unitOfWork.InitializeDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        _unitOfWork.Dispose();
        if (File.Exists(_testDbPath))
        {
       File.Delete(_testDbPath);
    }
        return Task.CompletedTask;
}

    [Fact]
    public async Task CreateTemplate_ShouldReturnValidId()
    {
        // Arrange
        var template = new ReportTemplate
        {
        Name = "Test Template",
       Description = "A test template",
    PaperSize = PaperSize.A4
        };

 // Act
        var id = await _unitOfWork.Templates.CreateAsync(template);

        // Assert
        Assert.True(id > 0);
    }

    [Fact]
    public async Task GetById_ShouldReturnTemplate()
    {
        // Arrange
        var template = new ReportTemplate
      {
        Name = "Test Template 2",
       Description = "Another test template"
        };
        var id = await _unitOfWork.Templates.CreateAsync(template);

   // Act
        var retrieved = await _unitOfWork.Templates.GetByIdAsync(id);

        // Assert
        Assert.NotNull(retrieved);
 Assert.Equal("Test Template 2", retrieved.Name);
    }

  [Fact]
    public async Task GetByName_ShouldReturnTemplate()
    {
        // Arrange
        var template = new ReportTemplate { Name = "Unique Name Template" };
        await _unitOfWork.Templates.CreateAsync(template);

 // Act
        var retrieved = await _unitOfWork.Templates.GetByNameAsync("Unique Name Template");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("Unique Name Template", retrieved.Name);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllActiveTemplates()
    {
   // Arrange - default template is created during initialization
   var template1 = new ReportTemplate { Name = "Template 1" };
        var template2 = new ReportTemplate { Name = "Template 2" };
      await _unitOfWork.Templates.CreateAsync(template1);
     await _unitOfWork.Templates.CreateAsync(template2);

      // Act
      var templates = await _unitOfWork.Templates.GetAllAsync();

        // Assert
        Assert.True(templates.Count() >= 2);
    }

    [Fact]
    public async Task UpdateTemplate_ShouldPersistChanges()
    {
        // Arrange
        var template = new ReportTemplate { Name = "Original Name" };
        var id = await _unitOfWork.Templates.CreateAsync(template);
   template.Id = id;
        template.Name = "Updated Name";

        // Act
    await _unitOfWork.Templates.UpdateAsync(template);
     var retrieved = await _unitOfWork.Templates.GetByIdAsync(id);

        // Assert
        Assert.NotNull(retrieved);
  Assert.Equal("Updated Name", retrieved.Name);
    }

    [Fact]
    public async Task DeleteTemplate_ShouldRemoveTemplate()
    {
        // Arrange
     var template = new ReportTemplate { Name = "To Be Deleted" };
        var id = await _unitOfWork.Templates.CreateAsync(template);

        // Act
        var result = await _unitOfWork.Templates.DeleteAsync(id);
      var retrieved = await _unitOfWork.Templates.GetByIdAsync(id);

        // Assert
   Assert.True(result);
  Assert.Null(retrieved);
    }

 [Fact]
    public async Task SetDefault_ShouldUpdateDefaultTemplate()
    {
        // Arrange
        var template = new ReportTemplate { Name = "New Default" };
  var id = await _unitOfWork.Templates.CreateAsync(template);

  // Act
 await _unitOfWork.Templates.SetDefaultAsync(id);
        var defaultTemplate = await _unitOfWork.Templates.GetDefaultAsync();

        // Assert
        Assert.NotNull(defaultTemplate);
 Assert.Equal(id, defaultTemplate.Id);
    }

    [Fact]
    public async Task DuplicateTemplate_ShouldCreateCopy()
    {
 // Arrange
        var original = new ReportTemplate
        {
         Name = "Original Template",
            Description = "Original description",
       Header = new HeaderSection
            {
 IsVisible = true,
             Lines = new List<HeaderLine>
   {
    new() { Order = 1, Text = "Header Line 1" }
       }
      }
        };
     var originalId = await _unitOfWork.Templates.CreateAsync(original);

        // Act
        var duplicateId = await _unitOfWork.Templates.DuplicateAsync(originalId, "Duplicated Template");
        var duplicate = await _unitOfWork.Templates.GetByIdAsync(duplicateId);

    // Assert
    Assert.NotNull(duplicate);
        Assert.Equal("Duplicated Template", duplicate.Name);
        Assert.NotEqual(originalId, duplicateId);
        Assert.NotNull(duplicate.Header);
    }

    [Fact]
    public async Task TemplateWithFullStructure_ShouldPersistAllSections()
    {
        // Arrange
        var template = new ReportTemplate
        {
  Name = "Full Template",
   Header = new HeaderSection
            {
          IsVisible = true,
         Height = 100,
 LeftLogo = new LogoPlacement { IsVisible = true, Width = 80, Height = 80 },
         RightLogo = new LogoPlacement { IsVisible = true, Width = 80, Height = 80 },
           Lines = new List<HeaderLine>
      {
       new() { Order = 1, Text = "Institution Name", FontSize = 16, IsBold = true },
    new() { Order = 2, Text = "Department", FontSize = 14 }
           }
            },
            Content = new ContentSection
 {
   InfoFields = new InfoFieldsLayout
   {
            IsVisible = true,
      ColumnsCount = 2,
       Fields = new List<InfoField>
               {
new() { Label = "Name", PlaceholderKey = "{PatientName}", Column = 0, Order = 1 },
  new() { Label = "MRN", PlaceholderKey = "{PatientMRN}", Column = 1, Order = 1 }
       }
   },
    ImageGrid = new ImageGridLayout
           {
         IsVisible = true,
     Columns = 2,
  Rows = 2,
        MaxImages = 4
                },
    Results = new ResultSection
     {
              IsVisible = true,
    Label = "Hasil Pemeriksaan"
       }
       },
    Footer = new FooterSection
        {
 IsVisible = true,
         Signature = new SignatureBlock
 {
  IsVisible = true,
           TitleLabel = "Dokter"
         },
           DateLocation = new DateLocationBlock
    {
         IsVisible = true,
              CityName = "Jakarta"
   }
            }
        };

   // Act
        var id = await _unitOfWork.Templates.CreateAsync(template);
        var retrieved = await _unitOfWork.Templates.GetByIdAsync(id);

        // Assert
    Assert.NotNull(retrieved);
        Assert.NotNull(retrieved.Header);
        Assert.Equal(2, retrieved.Header.Lines.Count);
        Assert.NotNull(retrieved.Content);
        Assert.Equal(2, retrieved.Content.InfoFields.Fields.Count);
   Assert.Equal(2, retrieved.Content.ImageGrid.Columns);
        Assert.NotNull(retrieved.Footer);
        Assert.True(retrieved.Footer.Signature.IsVisible);
    }
}
