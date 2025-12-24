using System.Text.Json;
using Microsoft.Data.Sqlite;
using ReportBuilder.Core.Models;

namespace ReportBuilder.Core.Storage;

/// <summary>
/// SQLite implementation of the settings repository.
/// </summary>
public class SqliteSettingsRepository : ISettingsRepository
{
    private readonly string _connectionString;

    public SqliteSettingsRepository(string connectionString)
    {
        _connectionString = connectionString;
  }

    public async Task<string?> GetAsync(string key)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = "SELECT Value FROM Settings WHERE Key = @Key";
        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Key", key);

     var result = await command.ExecuteScalarAsync();
        return result as string;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
     var value = await GetAsync(key);
   if (string.IsNullOrEmpty(value)) return default;

   try
        {
            if (typeof(T) == typeof(string))
         return (T)(object)value;

            if (typeof(T) == typeof(int))
      return (T)(object)int.Parse(value);

       if (typeof(T) == typeof(bool))
       return (T)(object)bool.Parse(value);

            if (typeof(T) == typeof(double))
                return (T)(object)double.Parse(value);

      if (typeof(T) == typeof(float))
   return (T)(object)float.Parse(value);

      return JsonSerializer.Deserialize<T>(value);
}
        catch
        {
      return default;
        }
    }

    public async Task SetAsync(string key, string value, string category = "General")
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
     INSERT INTO Settings (Key, Value, Category, UpdatedAt)
 VALUES (@Key, @Value, @Category, @UpdatedAt)
            ON CONFLICT(Key) DO UPDATE SET Value = @Value, Category = @Category, UpdatedAt = @UpdatedAt";

 await using var command = new SqliteCommand(sql, connection);
command.Parameters.AddWithValue("@Key", key);
    command.Parameters.AddWithValue("@Value", value);
        command.Parameters.AddWithValue("@Category", category);
 command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync();
}

    public async Task SetAsync<T>(string key, T value, string category = "General")
    {
 string stringValue;

        if (value is string str)
 stringValue = str;
  else if (value is int or bool or double or float)
            stringValue = value.ToString()!;
   else
  stringValue = JsonSerializer.Serialize(value);

await SetAsync(key, stringValue, category);
    }

    public async Task<IEnumerable<ReportBuilderSettings>> GetByCategoryAsync(string category)
    {
        await using var connection = new SqliteConnection(_connectionString);
   await connection.OpenAsync();

        const string sql = "SELECT Id, Key, Value, Category, UpdatedAt FROM Settings WHERE Category = @Category";
        var settings = new List<ReportBuilderSettings>();

        await using var command = new SqliteCommand(sql, connection);
     command.Parameters.AddWithValue("@Category", category);

     await using var reader = await command.ExecuteReaderAsync();
      while (await reader.ReadAsync())
        {
     settings.Add(new ReportBuilderSettings
            {
  Id = reader.GetInt32(0),
    Key = reader.GetString(1),
         Value = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Category = reader.GetString(3),
      UpdatedAt = DateTime.Parse(reader.GetString(4))
     });
        }

     return settings;
    }

 public async Task<bool> DeleteAsync(string key)
    {
        await using var connection = new SqliteConnection(_connectionString);
  await connection.OpenAsync();

        const string sql = "DELETE FROM Settings WHERE Key = @Key";
     await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Key", key);

        var rows = await command.ExecuteNonQueryAsync();
     return rows > 0;
    }
}

/// <summary>
/// SQLite implementation of the placeholder repository.
/// </summary>
public class SqlitePlaceholderRepository : IPlaceholderRepository
{
    private readonly string _connectionString;

    public SqlitePlaceholderRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<PlaceholderDefinition>> GetAllAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = "SELECT Id, Key, DisplayName, Category, DataType, DefaultValue, Format, IsSystem FROM PlaceholderDefinitions ORDER BY Category, DisplayName";
    return await LoadPlaceholdersAsync(connection, sql);
    }

    public async Task<IEnumerable<PlaceholderDefinition>> GetByCategoryAsync(string category)
    {
      await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

   const string sql = "SELECT Id, Key, DisplayName, Category, DataType, DefaultValue, Format, IsSystem FROM PlaceholderDefinitions WHERE Category = @Category ORDER BY DisplayName";
        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Category", category);

        return await LoadPlaceholdersFromCommandAsync(command);
    }

    public async Task<PlaceholderDefinition?> GetByKeyAsync(string key)
    {
        await using var connection = new SqliteConnection(_connectionString);
  await connection.OpenAsync();

     const string sql = "SELECT Id, Key, DisplayName, Category, DataType, DefaultValue, Format, IsSystem FROM PlaceholderDefinitions WHERE Key = @Key";
        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Key", key);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

     return MapReaderToPlaceholder(reader);
  }

 public async Task<int> CreateAsync(PlaceholderDefinition placeholder)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
    INSERT INTO PlaceholderDefinitions (Key, DisplayName, Category, DataType, DefaultValue, Format, IsSystem)
          VALUES (@Key, @DisplayName, @Category, @DataType, @DefaultValue, @Format, @IsSystem);
       SELECT last_insert_rowid();";

     await using var command = new SqliteCommand(sql, connection);
      command.Parameters.AddWithValue("@Key", placeholder.Key);
  command.Parameters.AddWithValue("@DisplayName", placeholder.DisplayName);
        command.Parameters.AddWithValue("@Category", placeholder.Category);
        command.Parameters.AddWithValue("@DataType", (int)placeholder.DataType);
  command.Parameters.AddWithValue("@DefaultValue", placeholder.DefaultValue ?? (object)DBNull.Value);
    command.Parameters.AddWithValue("@Format", placeholder.Format ?? (object)DBNull.Value);
   command.Parameters.AddWithValue("@IsSystem", placeholder.IsSystem ? 1 : 0);

        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task<bool> UpdateAsync(PlaceholderDefinition placeholder)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
  UPDATE PlaceholderDefinitions SET
       DisplayName = @DisplayName, Category = @Category, DataType = @DataType,
       DefaultValue = @DefaultValue, Format = @Format, IsSystem = @IsSystem
            WHERE Key = @Key";

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Key", placeholder.Key);
   command.Parameters.AddWithValue("@DisplayName", placeholder.DisplayName);
        command.Parameters.AddWithValue("@Category", placeholder.Category);
    command.Parameters.AddWithValue("@DataType", (int)placeholder.DataType);
command.Parameters.AddWithValue("@DefaultValue", placeholder.DefaultValue ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Format", placeholder.Format ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@IsSystem", placeholder.IsSystem ? 1 : 0);

        var rows = await command.ExecuteNonQueryAsync();
      return rows > 0;
    }

    public async Task<bool> DeleteAsync(string key)
    {
        await using var connection = new SqliteConnection(_connectionString);
  await connection.OpenAsync();

        const string sql = "DELETE FROM PlaceholderDefinitions WHERE Key = @Key AND IsSystem = 0";
        await using var command = new SqliteCommand(sql, connection);
    command.Parameters.AddWithValue("@Key", key);

        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    private async Task<List<PlaceholderDefinition>> LoadPlaceholdersAsync(SqliteConnection connection, string sql)
    {
        await using var command = new SqliteCommand(sql, connection);
        return await LoadPlaceholdersFromCommandAsync(command);
    }

    private async Task<List<PlaceholderDefinition>> LoadPlaceholdersFromCommandAsync(SqliteCommand command)
  {
        var placeholders = new List<PlaceholderDefinition>();
        await using var reader = await command.ExecuteReaderAsync();

   while (await reader.ReadAsync())
   {
            placeholders.Add(MapReaderToPlaceholder(reader));
     }

  return placeholders;
    }

    private static PlaceholderDefinition MapReaderToPlaceholder(SqliteDataReader reader)
    {
        return new PlaceholderDefinition
   {
     Id = reader.GetInt32(0),
            Key = reader.GetString(1),
            DisplayName = reader.GetString(2),
    Category = reader.GetString(3),
 DataType = (PlaceholderDataType)reader.GetInt32(4),
   DefaultValue = reader.IsDBNull(5) ? null : reader.GetString(5),
     Format = reader.IsDBNull(6) ? null : reader.GetString(6),
     IsSystem = reader.GetInt32(7) == 1
        };
    }
}
