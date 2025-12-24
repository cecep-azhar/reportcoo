using ReportBuilder.Core.Models;

namespace ReportBuilder.Core.Storage;

/// <summary>
/// Interface for template repository operations.
/// </summary>
public interface ITemplateRepository
{
    // Template CRUD
    Task<ReportTemplate?> GetByIdAsync(int id);
    Task<ReportTemplate?> GetByNameAsync(string name);
    Task<ReportTemplate?> GetDefaultAsync();
    Task<IEnumerable<ReportTemplate>> GetAllAsync(bool includeInactive = false);
    Task<int> CreateAsync(ReportTemplate template);
    Task<bool> UpdateAsync(ReportTemplate template);
    Task<bool> DeleteAsync(int id);
    Task<bool> SetDefaultAsync(int id);
    Task<bool> ExistsAsync(string name);
    
 // Template duplication
    Task<int> DuplicateAsync(int id, string newName);
}

/// <summary>
/// Interface for settings repository operations.
/// </summary>
public interface ISettingsRepository
{
    Task<string?> GetAsync(string key);
    Task<T?> GetAsync<T>(string key);
 Task SetAsync(string key, string value, string category = "General");
    Task SetAsync<T>(string key, T value, string category = "General");
  Task<IEnumerable<ReportBuilderSettings>> GetByCategoryAsync(string category);
    Task<bool> DeleteAsync(string key);
}

/// <summary>
/// Interface for placeholder repository operations.
/// </summary>
public interface IPlaceholderRepository
{
    Task<IEnumerable<PlaceholderDefinition>> GetAllAsync();
    Task<IEnumerable<PlaceholderDefinition>> GetByCategoryAsync(string category);
    Task<PlaceholderDefinition?> GetByKeyAsync(string key);
    Task<int> CreateAsync(PlaceholderDefinition placeholder);
    Task<bool> UpdateAsync(PlaceholderDefinition placeholder);
    Task<bool> DeleteAsync(string key);
}

/// <summary>
/// Combined unit of work interface for all repositories.
/// </summary>
public interface IReportBuilderUnitOfWork : IDisposable, IAsyncDisposable
{
    ITemplateRepository Templates { get; }
    ISettingsRepository Settings { get; }
    IPlaceholderRepository Placeholders { get; }
    
    Task InitializeDatabaseAsync();
    Task<bool> BackupDatabaseAsync(string backupPath);
}
