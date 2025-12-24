using ReportBuilder.Core.Models;

namespace ReportBuilder.Core.Builder;

/// <summary>
/// Fluent builder for creating and configuring report templates.
/// </summary>
public class TemplateBuilder
{
    private readonly ReportTemplate _template;

    public TemplateBuilder()
    {
  _template = new ReportTemplate();
    }

    public TemplateBuilder(ReportTemplate existingTemplate)
    {
        _template = existingTemplate;
    }

  #region Basic Settings

 public TemplateBuilder WithName(string name)
    {
        _template.Name = name;
        return this;
    }

    public TemplateBuilder WithDescription(string description)
    {
       _template.Description = description;
        return this;
  }

public TemplateBuilder WithPaperSize(PaperSize size)
    {
      _template.PaperSize = size;
  return this;
    }

    public TemplateBuilder WithOrientation(PageOrientation orientation)
 {
        _template.Orientation = orientation;
        return this;
    }

    public TemplateBuilder WithMargins(float all)
  {
        _template.Margins = new PageMargins(all);
        return this;
    }

    public TemplateBuilder WithMargins(float top, float right, float bottom, float left)
    {
        _template.Margins = new PageMargins(top, right, bottom, left);
        return this;
    }

    #endregion

    #region Header Section

    public TemplateBuilder WithHeader(Action<HeaderSectionBuilder> configure)
    {
        var builder = new HeaderSectionBuilder();
        configure(builder);
        _template.Header = builder.Build();
    return this;
    }

    public TemplateBuilder WithoutHeader()
    {
        _template.Header = null;
        return this;
    }

    #endregion

    #region Content Section

    public TemplateBuilder WithContent(Action<ContentSectionBuilder> configure)
    {
        var builder = new ContentSectionBuilder();
  configure(builder);
    _template.Content = builder.Build();
        return this;
    }

    #endregion

    #region Footer Section

    public TemplateBuilder WithFooter(Action<FooterSectionBuilder> configure)
    {
     var builder = new FooterSectionBuilder();
        configure(builder);
        _template.Footer = builder.Build();
     return this;
    }

    public TemplateBuilder WithoutFooter()
    {
  _template.Footer = null;
        return this;
    }

    #endregion

    public ReportTemplate Build() => _template;
}

/// <summary>
/// Builder for header sections.
/// </summary>
public class HeaderSectionBuilder
{
    private readonly HeaderSection _header = new();

  public HeaderSectionBuilder WithHeight(float height)
  {
        _header.Height = height;
  return this;
}

    public HeaderSectionBuilder WithLeftLogo(float width = 80, float height = 80, string? path = null)
    {
 _header.LeftLogo = new LogoPlacement
      {
            IsVisible = true,
  Width = width,
       Height = height,
     ImagePath = path
  };
        return this;
  }

    public HeaderSectionBuilder WithRightLogo(float width = 80, float height = 80, string? path = null)
    {
        _header.RightLogo = new LogoPlacement
        {
      IsVisible = true,
    Width = width,
      Height = height,
            ImagePath = path
        };
        return this;
 }

    public HeaderSectionBuilder WithoutLeftLogo()
    {
    _header.LeftLogo.IsVisible = false;
        return this;
    }

    public HeaderSectionBuilder WithoutRightLogo()
  {
        _header.RightLogo.IsVisible = false;
        return this;
    }

    public HeaderSectionBuilder AddLine(string text, float fontSize = 12, bool bold = false, TextAlignment alignment = TextAlignment.Center)
    {
        _header.Lines.Add(new HeaderLine
        {
   Order = _header.Lines.Count + 1,
            Text = text,
     FontSize = fontSize,
   IsBold = bold,
            Alignment = alignment,
 IsVisible = true
  });
        return this;
    }

    public HeaderSectionBuilder AddPlaceholderLine(string placeholderKey, float fontSize = 12, bool bold = false, TextAlignment alignment = TextAlignment.Center)
    {
      _header.Lines.Add(new HeaderLine
        {
Order = _header.Lines.Count + 1,
       PlaceholderKey = placeholderKey,
   FontSize = fontSize,
            IsBold = bold,
            Alignment = alignment,
     IsVisible = true
        });
        return this;
    }

    public HeaderSectionBuilder WithBorderBottom(bool show = true, string color = "#000000", float thickness = 1)
    {
     _header.ShowBorderBottom = show;
        _header.BorderColor = color;
        _header.BorderThickness = thickness;
        return this;
    }

    public HeaderSection Build() => _header;
}

/// <summary>
/// Builder for content sections.
/// </summary>
public class ContentSectionBuilder
{
    private readonly ContentSection _content = new();

    public ContentSectionBuilder WithInfoFields(Action<InfoFieldsBuilder> configure)
    {
        var builder = new InfoFieldsBuilder();
        configure(builder);
        _content.InfoFields = builder.Build();
        return this;
    }

    public ContentSectionBuilder WithImageGrid(int columns = 2, int rows = 2, int maxImages = 4, float spacing = 10)
    {
    _content.ImageGrid = new ImageGridLayout
        {
            IsVisible = true,
            Columns = columns,
     Rows = rows,
        MaxImages = maxImages,
    ImageSpacing = spacing,
 ScaleMode = ImageScaleMode.Uniform
     };
        return this;
    }

    public ContentSectionBuilder WithImageGrid(Action<ImageGridBuilder> configure)
    {
        var builder = new ImageGridBuilder();
        configure(builder);
      _content.ImageGrid = builder.Build();
        return this;
 }

    public ContentSectionBuilder WithResults(string label = "Hasil Pemeriksaan", bool labelBold = true)
    {
        _content.Results = new ResultSection
        {
        IsVisible = true,
            Label = label,
   LabelBold = labelBold,
            PlaceholderKey = PlaceholderKeys.ExamResult
        };
        return this;
    }

    public ContentSectionBuilder WithoutResults()
    {
        _content.Results.IsVisible = false;
   return this;
    }

    public ContentSection Build() => _content;
}

/// <summary>
/// Builder for info fields layout.
/// </summary>
public class InfoFieldsBuilder
{
    private readonly InfoFieldsLayout _layout = new();

    public InfoFieldsBuilder WithColumns(int columns)
    {
        _layout.ColumnsCount = columns;
        return this;
    }

    public InfoFieldsBuilder AddField(string label, string placeholderKey, int column = 0)
    {
     _layout.Fields.Add(new InfoField
        {
            Label = label,
   PlaceholderKey = placeholderKey,
       Column = column,
          Order = _layout.Fields.Count(f => f.Column == column) + 1,
            IsVisible = true
     });
        return this;
    }

    public InfoFieldsBuilder WithFontSize(float size)
    {
   _layout.FontSize = size;
        return this;
    }

    public InfoFieldsLayout Build() => _layout;
}

/// <summary>
/// Builder for image grid layout.
/// </summary>
public class ImageGridBuilder
{
    private readonly ImageGridLayout _grid = new();

    public ImageGridBuilder WithGrid(int columns, int rows)
    {
        _grid.Columns = columns;
        _grid.Rows = rows;
        _grid.MaxImages = columns * rows;
   return this;
    }

    public ImageGridBuilder WithMaxImages(int max)
    {
   _grid.MaxImages = max;
   return this;
    }

    public ImageGridBuilder WithSpacing(float spacing)
    {
        _grid.ImageSpacing = spacing;
        return this;
    }

    public ImageGridBuilder WithBorder(string color = "#CCCCCC", float thickness = 1)
    {
      _grid.ShowImageBorder = true;
   _grid.BorderColor = color;
        _grid.BorderThickness = thickness;
        return this;
    }

    public ImageGridBuilder WithScaleMode(ImageScaleMode mode)
 {
        _grid.ScaleMode = mode;
        return this;
    }

    public ImageGridLayout Build() => _grid;
}

/// <summary>
/// Builder for footer sections.
/// </summary>
public class FooterSectionBuilder
{
    private readonly FooterSection _footer = new();

    public FooterSectionBuilder WithHeight(float height)
    {
        _footer.Height = height;
        return this;
    }

    public FooterSectionBuilder WithSignature(string titleLabel = "Dokter Pemeriksa", float spaceHeight = 60)
    {
  _footer.Signature = new SignatureBlock
        {
        IsVisible = true,
            TitleLabel = titleLabel,
            SignatureSpaceHeight = spaceHeight,
            NamePlaceholder = PlaceholderKeys.DoctorName,
     Position = HorizontalPosition.Right
        };
        return this;
    }

    public FooterSectionBuilder WithDateLocation(string? cityName = null, string dateFormat = "dd MMMM yyyy")
    {
        _footer.DateLocation = new DateLocationBlock
        {
            IsVisible = true,
   CityName = cityName ?? string.Empty,
            DateFormat = dateFormat,
     Position = HorizontalPosition.Right
        };
        return this;
    }

    public FooterSectionBuilder WithBorderTop(bool show = true, string color = "#000000", float thickness = 1)
    {
        _footer.ShowBorderTop = show;
 _footer.BorderColor = color;
        _footer.BorderThickness = thickness;
        return this;
    }

public FooterSectionBuilder AddPageNumber()
    {
     _footer.AdditionalElements.Add(new FooterElement
    {
      Order = _footer.AdditionalElements.Count + 1,
     Type = FooterElementType.PageNumber,
            Position = HorizontalPosition.Center,
     IsVisible = true
});
   return this;
    }

    public FooterSection Build() => _footer;
}
