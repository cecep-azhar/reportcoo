using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ReportBuilder.Core.Models;
using ReportBuilder.Core.Storage;

namespace ReportBuilder.WinUI.Controls;

/// <summary>
/// A control for designing and editing report templates.
/// </summary>
public sealed partial class TemplateDesigner : UserControl
{
    private IReportBuilderUnitOfWork? _unitOfWork;
    private ReportTemplate? _currentTemplate;
    private bool _isDirty;

    /// <summary>
    /// Event raised when a template is saved.
    /// </summary>
    public event EventHandler<ReportTemplate>? TemplateSaved;

    /// <summary>
    /// Event raised when the template changes.
    /// </summary>
    public event EventHandler<ReportTemplate?>? TemplateChanged;

    public TemplateDesigner()
    {
 this.InitializeComponent();
    }

    /// <summary>
    /// Initializes the designer with a database connection.
    /// </summary>
    public async Task InitializeAsync(string databasePath)
    {
        _unitOfWork = new SqliteUnitOfWork(databasePath);
        await _unitOfWork.InitializeDatabaseAsync();
        await LoadTemplatesAsync();
    }

    /// <summary>
    /// Initializes the designer with an existing unit of work.
 /// </summary>
  public async Task InitializeAsync(IReportBuilderUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
 await LoadTemplatesAsync();
    }

    /// <summary>
    /// Gets the current template being edited.
    /// </summary>
    public ReportTemplate? CurrentTemplate => _currentTemplate;

    /// <summary>
    /// Gets whether there are unsaved changes.
    /// </summary>
    public bool IsDirty => _isDirty;

  private async Task LoadTemplatesAsync()
    {
        if (_unitOfWork == null) return;

        var templates = await _unitOfWork.Templates.GetAllAsync();
        TemplateComboBox.ItemsSource = templates.Select(t => new ComboBoxItem 
        { 
   Content = t.Name, 
            Tag = t.Id 
    }).ToList();

        // Select default template
      var defaultTemplate = templates.FirstOrDefault(t => t.IsDefault) ?? templates.FirstOrDefault();
        if (defaultTemplate != null)
        {
 var item = (TemplateComboBox.ItemsSource as List<ComboBoxItem>)?
                .FirstOrDefault(i => (int)i.Tag == defaultTemplate.Id);
            if (item != null)
   {
     TemplateComboBox.SelectedItem = item;
     }
        }
    }

  private async void TemplateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_unitOfWork == null) return;
      if (TemplateComboBox.SelectedItem is not ComboBoxItem selectedItem) return;

        // Check for unsaved changes
        if (_isDirty)
        {
            var result = await ShowUnsavedChangesDialogAsync();
            if (result == ContentDialogResult.Primary)
  {
          await SaveCurrentTemplateAsync();
     }
        else if (result == ContentDialogResult.None)
            {
                // Cancel - revert selection
    return;
        }
     }

        var templateId = (int)selectedItem.Tag;
        _currentTemplate = await _unitOfWork.Templates.GetByIdAsync(templateId);
        _isDirty = false;

        UpdatePreview();
        TemplateChanged?.Invoke(this, _currentTemplate);
    }

    private void SectionTreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
    {
    if (args.InvokedItem is TreeViewNode node && node.Tag is string sectionTag)
 {
            NavigateToPropertyEditor(sectionTag);
        }
    }

  private void NavigateToPropertyEditor(string sectionTag)
    {
        if (_currentTemplate == null) return;

        PropertyPanelTitle.Text = sectionTag switch
{
            "PageSize" => "Ukuran Kertas",
       "Margins" => "Margin Halaman",
       "LeftLogo" => "Logo Kiri",
          "RightLogo" => "Logo Kanan",
         "HeaderLines" => "Teks Header",
            "InfoFields" => "Field Informasi",
            "ImageGrid" => "Grid Gambar",
   "Results" => "Hasil Pemeriksaan",
         "DateLocation" => "Tanggal & Lokasi",
          "Signature" => "Tanda Tangan",
            _ => "Properties"
        };

    // Navigate to appropriate property editor page
  // For simplicity, we'll use a single property panel that updates based on selection
        UpdatePropertyPanel(sectionTag);
    }

    private void UpdatePropertyPanel(string sectionTag)
    {
        // This would normally navigate to different property editor pages
        // For now, we'll create a simple inline editor
        var panel = new StackPanel { Spacing = 12 };

        switch (sectionTag)
        {
            case "PageSize":
         CreatePageSizeEditor(panel);
         break;
   case "Margins":
                CreateMarginsEditor(panel);
     break;
       case "LeftLogo":
            case "RightLogo":
    CreateLogoEditor(panel, sectionTag == "LeftLogo");
         break;
          case "HeaderLines":
  CreateHeaderLinesEditor(panel);
     break;
 case "ImageGrid":
     CreateImageGridEditor(panel);
                break;
        default:
    panel.Children.Add(new TextBlock { Text = "Editor not implemented yet" });
                break;
        }

        PropertyEditorFrame.Content = panel;
    }

    private void CreatePageSizeEditor(StackPanel panel)
    {
        if (_currentTemplate == null) return;

        var sizeCombo = new ComboBox
{
    Header = "Ukuran Kertas",
            HorizontalAlignment = HorizontalAlignment.Stretch,
      ItemsSource = Enum.GetValues<PaperSize>().Select(s => s.ToString()).ToList(),
            SelectedItem = _currentTemplate.PaperSize.ToString()
        };
        sizeCombo.SelectionChanged += (s, e) =>
        {
        if (Enum.TryParse<PaperSize>(sizeCombo.SelectedItem?.ToString(), out var size))
  {
                _currentTemplate.PaperSize = size;
      MarkDirty();
          UpdatePreview();
          }
 };
        panel.Children.Add(sizeCombo);

        var orientationCombo = new ComboBox
     {
            Header = "Orientasi",
 HorizontalAlignment = HorizontalAlignment.Stretch,
   ItemsSource = new[] { "Portrait", "Landscape" },
          SelectedItem = _currentTemplate.Orientation.ToString()
        };
  orientationCombo.SelectionChanged += (s, e) =>
        {
 if (Enum.TryParse<PageOrientation>(orientationCombo.SelectedItem?.ToString(), out var orientation))
       {
             _currentTemplate.Orientation = orientation;
           MarkDirty();
      UpdatePreview();
     }
        };
        panel.Children.Add(orientationCombo);
    }

    private void CreateMarginsEditor(StackPanel panel)
    {
        if (_currentTemplate == null) return;

        var margins = _currentTemplate.Margins;

 var topBox = CreateNumberBox("Atas (mm)", margins.Top, v => { margins.Top = v; MarkDirty(); UpdatePreview(); });
        var bottomBox = CreateNumberBox("Bawah (mm)", margins.Bottom, v => { margins.Bottom = v; MarkDirty(); UpdatePreview(); });
      var leftBox = CreateNumberBox("Kiri (mm)", margins.Left, v => { margins.Left = v; MarkDirty(); UpdatePreview(); });
        var rightBox = CreateNumberBox("Kanan (mm)", margins.Right, v => { margins.Right = v; MarkDirty(); UpdatePreview(); });

        panel.Children.Add(topBox);
        panel.Children.Add(bottomBox);
        panel.Children.Add(leftBox);
        panel.Children.Add(rightBox);
    }

    private void CreateLogoEditor(StackPanel panel, bool isLeft)
    {
    if (_currentTemplate?.Header == null) return;

        var logo = isLeft ? _currentTemplate.Header.LeftLogo : _currentTemplate.Header.RightLogo;

        var visibleToggle = new ToggleSwitch
        {
   Header = "Tampilkan",
IsOn = logo.IsVisible
        };
        visibleToggle.Toggled += (s, e) =>
  {
     logo.IsVisible = visibleToggle.IsOn;
    MarkDirty();
  UpdatePreview();
    };
      panel.Children.Add(visibleToggle);

  var widthBox = CreateNumberBox("Lebar (mm)", logo.Width, v => { logo.Width = v; MarkDirty(); UpdatePreview(); });
        var heightBox = CreateNumberBox("Tinggi (mm)", logo.Height, v => { logo.Height = v; MarkDirty(); UpdatePreview(); });
 panel.Children.Add(widthBox);
     panel.Children.Add(heightBox);

        var pathBox = new TextBox
    {
            Header = "Path Gambar",
        Text = logo.ImagePath ?? "",
    HorizontalAlignment = HorizontalAlignment.Stretch
      };
   pathBox.TextChanged += (s, e) =>
        {
  logo.ImagePath = pathBox.Text;
            MarkDirty();
};
        panel.Children.Add(pathBox);

   var browseButton = new Button { Content = "Pilih Gambar..." };
        browseButton.Click += async (s, e) =>
  {
         var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");

 var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
      WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
  {
       pathBox.Text = file.Path;
    logo.ImagePath = file.Path;
        logo.ImageData = await System.IO.File.ReadAllBytesAsync(file.Path);
       MarkDirty();
       UpdatePreview();
     }
  };
        panel.Children.Add(browseButton);
    }

    private void CreateHeaderLinesEditor(StackPanel panel)
    {
   if (_currentTemplate?.Header == null) return;

        var addButton = new Button { Content = "Tambah Baris" };
  addButton.Click += (s, e) =>
        {
        _currentTemplate.Header.Lines.Add(new HeaderLine
  {
         Order = _currentTemplate.Header.Lines.Count + 1,
    Text = "Baris Baru",
                FontSize = 12,
      Alignment = TextAlignment.Center,
           IsVisible = true
       });
            MarkDirty();
            CreateHeaderLinesEditor(panel);
   UpdatePreview();
        };
        panel.Children.Add(addButton);

        foreach (var line in _currentTemplate.Header.Lines.OrderBy(l => l.Order))
        {
        var linePanel = new StackPanel 
      { 
       Spacing = 4, 
     Padding = new Thickness(0, 8, 0, 8),
   BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                BorderThickness = new Thickness(0, 0, 0, 1)
            };

     var textBox = new TextBox
       {
        Header = $"Baris {line.Order}",
                Text = string.IsNullOrEmpty(line.PlaceholderKey) ? line.Text : line.PlaceholderKey,
          HorizontalAlignment = HorizontalAlignment.Stretch
   };
            textBox.TextChanged += (s, e) =>
        {
 if (textBox.Text.StartsWith("{"))
        line.PlaceholderKey = textBox.Text;
else
      line.Text = textBox.Text;
      MarkDirty();
         UpdatePreview();
            };
       linePanel.Children.Add(textBox);

     var fontSizeBox = CreateNumberBox("Ukuran Font", line.FontSize, v => { line.FontSize = v; MarkDirty(); UpdatePreview(); });
       linePanel.Children.Add(fontSizeBox);

            var boldToggle = new CheckBox { Content = "Tebal", IsChecked = line.IsBold };
    boldToggle.Checked += (s, e) => { line.IsBold = true; MarkDirty(); UpdatePreview(); };
        boldToggle.Unchecked += (s, e) => { line.IsBold = false; MarkDirty(); UpdatePreview(); };
            linePanel.Children.Add(boldToggle);

  var deleteButton = new Button { Content = "Hapus", Tag = line };
          deleteButton.Click += (s, e) =>
   {
  _currentTemplate.Header.Lines.Remove(line);
                MarkDirty();
         CreateHeaderLinesEditor(panel);
   UpdatePreview();
          };
    linePanel.Children.Add(deleteButton);

            panel.Children.Add(linePanel);
   }
}

    private void CreateImageGridEditor(StackPanel panel)
  {
        if (_currentTemplate == null) return;

        var grid = _currentTemplate.Content.ImageGrid;

        var columnsBox = CreateNumberBox("Kolom", grid.Columns, v => { grid.Columns = (int)v; MarkDirty(); UpdatePreview(); });
        var rowsBox = CreateNumberBox("Baris", grid.Rows, v => { grid.Rows = (int)v; MarkDirty(); UpdatePreview(); });
        var maxBox = CreateNumberBox("Maks Gambar", grid.MaxImages, v => { grid.MaxImages = (int)v; MarkDirty(); UpdatePreview(); });
        var spacingBox = CreateNumberBox("Jarak (mm)", grid.ImageSpacing, v => { grid.ImageSpacing = v; MarkDirty(); UpdatePreview(); });

        panel.Children.Add(columnsBox);
        panel.Children.Add(rowsBox);
        panel.Children.Add(maxBox);
        panel.Children.Add(spacingBox);

        var borderToggle = new ToggleSwitch
        {
 Header = "Tampilkan Border",
          IsOn = grid.ShowImageBorder
     };
    borderToggle.Toggled += (s, e) =>
        {
            grid.ShowImageBorder = borderToggle.IsOn;
            MarkDirty();
            UpdatePreview();
        };
        panel.Children.Add(borderToggle);
    }

    private NumberBox CreateNumberBox(string header, float value, Action<float> onChanged)
    {
        var box = new NumberBox
        {
          Header = header,
  Value = value,
    SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
        HorizontalAlignment = HorizontalAlignment.Stretch
        };
        box.ValueChanged += (s, e) =>
        {
 if (!double.IsNaN(box.Value))
 {
    onChanged((float)box.Value);
}
     };
        return box;
    }

    private void MarkDirty()
    {
        _isDirty = true;
    }

    private void UpdatePreview()
    {
        if (_currentTemplate != null)
        {
      PreviewControl.SetTemplate(_currentTemplate);
        }
    }

    private async void NewTemplateButton_Click(object sender, RoutedEventArgs e)
    {
    var dialog = new ContentDialog
    {
      Title = "Template Baru",
PrimaryButtonText = "Buat",
         CloseButtonText = "Batal",
     XamlRoot = this.XamlRoot
      };

        var nameBox = new TextBox { Header = "Nama Template", PlaceholderText = "Masukkan nama..." };
      dialog.Content = nameBox;

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(nameBox.Text))
        {
_currentTemplate = new ReportTemplate
        {
        Name = nameBox.Text,
   Header = new HeaderSection
        {
       IsVisible = true,
  Lines = new List<HeaderLine>
        {
      new() { Order = 1, PlaceholderKey = PlaceholderKeys.InstitutionName, FontSize = 16, IsBold = true }
    }
        },
                Content = new ContentSection(),
      Footer = new FooterSection { IsVisible = true }
  };

            if (_unitOfWork != null)
            {
       await _unitOfWork.Templates.CreateAsync(_currentTemplate);
       await LoadTemplatesAsync();
    }

            _isDirty = false;
  UpdatePreview();
         TemplateChanged?.Invoke(this, _currentTemplate);
        }
    }

    private async void DuplicateTemplateButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentTemplate == null || _unitOfWork == null) return;

        var dialog = new ContentDialog
        {
            Title = "Duplikat Template",
       PrimaryButtonText = "Duplikat",
            CloseButtonText = "Batal",
            XamlRoot = this.XamlRoot
  };

        var nameBox = new TextBox { Header = "Nama Template Baru", Text = $"{_currentTemplate.Name} (Copy)" };
        dialog.Content = nameBox;

        var result = await dialog.ShowAsync();
     if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(nameBox.Text))
        {
        await _unitOfWork.Templates.DuplicateAsync(_currentTemplate.Id, nameBox.Text);
   await LoadTemplatesAsync();
        }
    }

    private async void DeleteTemplateButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentTemplate == null || _unitOfWork == null) return;

        var dialog = new ContentDialog
        {
 Title = "Hapus Template",
            Content = $"Apakah Anda yakin ingin menghapus template '{_currentTemplate.Name}'?",
            PrimaryButtonText = "Hapus",
            CloseButtonText = "Batal",
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
   await _unitOfWork.Templates.DeleteAsync(_currentTemplate.Id);
      _currentTemplate = null;
  _isDirty = false;
          await LoadTemplatesAsync();
        }
    }

    private async void SaveTemplateButton_Click(object sender, RoutedEventArgs e)
    {
        await SaveCurrentTemplateAsync();
    }

    private async Task SaveCurrentTemplateAsync()
    {
        if (_currentTemplate == null || _unitOfWork == null) return;

   if (_currentTemplate.Id == 0)
 {
await _unitOfWork.Templates.CreateAsync(_currentTemplate);
}
        else
   {
       await _unitOfWork.Templates.UpdateAsync(_currentTemplate);
        }

        _isDirty = false;
        TemplateSaved?.Invoke(this, _currentTemplate);
    await LoadTemplatesAsync();
    }

 private async void ExportTemplateButton_Click(object sender, RoutedEventArgs e)
    {
   if (_currentTemplate == null) return;

        var picker = new Windows.Storage.Pickers.FileSavePicker();
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
 picker.FileTypeChoices.Add("JSON", new[] { ".json" });
  picker.SuggestedFileName = _currentTemplate.Name;

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

     var file = await picker.PickSaveFileAsync();
      if (file != null)
     {
            var json = System.Text.Json.JsonSerializer.Serialize(_currentTemplate, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            await Windows.Storage.FileIO.WriteTextAsync(file, json);
        }
    }

    private void ZoomSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
  {
        ZoomLabel.Text = $"{(int)ZoomSlider.Value}%";
  var scale = ZoomSlider.Value / 100.0;
        PreviewControl.RenderTransform = new Microsoft.UI.Xaml.Media.ScaleTransform { ScaleX = scale, ScaleY = scale };
    }

    private void RefreshPreviewButton_Click(object sender, RoutedEventArgs e)
    {
      UpdatePreview();
    }

    private async Task<ContentDialogResult> ShowUnsavedChangesDialogAsync()
    {
        var dialog = new ContentDialog
      {
 Title = "Perubahan Belum Disimpan",
   Content = "Ada perubahan yang belum disimpan. Simpan sebelum melanjutkan?",
            PrimaryButtonText = "Simpan",
    SecondaryButtonText = "Jangan Simpan",
  CloseButtonText = "Batal",
   XamlRoot = this.XamlRoot
        };

        return await dialog.ShowAsync();
    }
}

/// <summary>
/// Static class to hold the main window reference for file pickers.
/// </summary>
public static class App
{
    public static Microsoft.UI.Xaml.Window? MainWindow { get; set; }
}
