using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using JobHunter.Domain.Interfaces;
using JobHunter.Domain.Models;

namespace JobHunter.Infrastructure.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly IDbContextFactory<JobHunterDbContext> _contextFactory;
    private Dictionary<string, List<string>>? _schemaCache;

    public AnalyticsRepository(IDbContextFactory<JobHunterDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    private async Task<string> GetConnectionStringAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return context.Database.GetConnectionString() ?? throw new InvalidOperationException("No connection string configured.");
    }

    private async Task EnsureSchemaCacheAsync()
    {
        if (_schemaCache != null) return;
        _schemaCache = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        var connStr = await GetConnectionStringAsync();
        using var conn = new SqlConnection(connStr);
        await conn.OpenAsync();

        const string sql = @"
            SELECT t.TABLE_NAME, c.COLUMN_NAME
            FROM INFORMATION_SCHEMA.TABLES t
            INNER JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME AND t.TABLE_SCHEMA = c.TABLE_SCHEMA
            WHERE t.TABLE_TYPE = 'BASE TABLE' AND t.TABLE_SCHEMA = 'dbo'
            ORDER BY t.TABLE_NAME, c.ORDINAL_POSITION";

        using var cmd = new SqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var table = reader.GetString(0);
            var column = reader.GetString(1);
            if (!_schemaCache.ContainsKey(table))
                _schemaCache[table] = new List<string>();
            _schemaCache[table].Add(column);
        }
    }

    private void ValidateIdentifier(string name, string kind)
    {
        if (string.IsNullOrWhiteSpace(name) || !System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
            throw new ArgumentException($"Invalid {kind} name: '{name}'");
    }

    private void ValidateTableExists(string tableName)
    {
        if (_schemaCache == null || !_schemaCache.ContainsKey(tableName))
            throw new ArgumentException($"Table '{tableName}' not found in database schema.");
    }

    private void ValidateColumnExists(string tableName, string columnName)
    {
        if (_schemaCache == null || !_schemaCache.ContainsKey(tableName) || !_schemaCache[tableName].Contains(columnName))
            throw new ArgumentException($"Column '{columnName}' not found in table '{tableName}'.");
    }

    public async Task<List<string>> GetTablesAsync()
    {
        await EnsureSchemaCacheAsync();
        return _schemaCache!.Keys.OrderBy(t => t).ToList();
    }

    public async Task<List<string>> GetColumnsAsync(string tableName)
    {
        await EnsureSchemaCacheAsync();
        ValidateIdentifier(tableName, "table");
        ValidateTableExists(tableName);
        return _schemaCache![tableName].ToList();
    }

    public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(AnalyticsQueryConfig config)
    {
        await EnsureSchemaCacheAsync();

        ValidateIdentifier(config.PrimaryTable, "table");
        ValidateTableExists(config.PrimaryTable);
        ValidateIdentifier(config.XAxisColumn, "column");

        var hasJoin = !string.IsNullOrWhiteSpace(config.JoinTable);
        var primaryAlias = "t1";
        var joinAlias = "t2";

        string ResolveColumn(string col, string context)
        {
            ValidateIdentifier(col, "column");
            if (hasJoin)
            {
                bool inPrimary = _schemaCache!.ContainsKey(config.PrimaryTable) && _schemaCache[config.PrimaryTable].Contains(col);
                bool inJoin = config.JoinTable != null && _schemaCache!.ContainsKey(config.JoinTable) && _schemaCache[config.JoinTable].Contains(col);

                if (inPrimary) return $"{primaryAlias}.[{col}]";
                if (inJoin) return $"{joinAlias}.[{col}]";
                throw new ArgumentException($"Column '{col}' not found in either table for {context}.");
            }
            else
            {
                ValidateColumnExists(config.PrimaryTable, col);
                return $"[{col}]";
            }
        }

        var xCol = ResolveColumn(config.XAxisColumn, "X-axis");

        string selectClause;
        string groupByClause = "";
        string orderByClause = $"ORDER BY {xCol}";

        if (config.AggregateFunction == AggregateFunction.None)
        {
            if (string.IsNullOrWhiteSpace(config.YAxisColumn))
            {
                selectClause = $"SELECT {xCol} AS [XValue], {xCol} AS [YValue]";
            }
            else
            {
                var yCol = ResolveColumn(config.YAxisColumn, "Y-axis");
                selectClause = $"SELECT {xCol} AS [XValue], {yCol} AS [YValue]";
            }
        }
        else
        {
            string aggExpr;
            if (config.AggregateFunction == AggregateFunction.Count)
            {
                aggExpr = "COUNT(*)";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(config.YAxisColumn))
                    throw new ArgumentException("Y-axis column is required for Sum/Avg/Min/Max aggregation.");
                var yCol = ResolveColumn(config.YAxisColumn, "Y-axis");
                aggExpr = $"{config.AggregateFunction.ToString().ToUpper()}({yCol})";
            }

            selectClause = $"SELECT {xCol} AS [XValue], {aggExpr} AS [YValue]";
            groupByClause = $"GROUP BY {xCol}";
        }

        var fromClause = hasJoin
            ? $"FROM [{config.PrimaryTable}] {primaryAlias}"
            : $"FROM [{config.PrimaryTable}]";

        var joinClause = "";
        if (hasJoin)
        {
            ValidateIdentifier(config.JoinTable!, "table");
            ValidateTableExists(config.JoinTable!);
            ValidateIdentifier(config.JoinColumnLeft ?? "", "join column left");
            ValidateIdentifier(config.JoinColumnRight ?? "", "join column right");

            var joinKeyword = config.JoinType switch
            {
                JoinType.Inner => "INNER JOIN",
                JoinType.Left => "LEFT JOIN",
                JoinType.Right => "RIGHT JOIN",
                JoinType.Full => "FULL OUTER JOIN",
                _ => "INNER JOIN"
            };

            joinClause = $"{joinKeyword} [{config.JoinTable}] {joinAlias} ON {primaryAlias}.[{config.JoinColumnLeft}] = {joinAlias}.[{config.JoinColumnRight}]";
        }

        var query = $"{selectClause} {fromClause} {joinClause} {groupByClause} {orderByClause}";

        var connStr = await GetConnectionStringAsync();
        using var conn = new SqlConnection(connStr);
        await conn.OpenAsync();
        using var cmd = new SqlCommand(query, conn);
        cmd.CommandTimeout = 30;

        var results = new List<Dictionary<string, object>>();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
            }
            results.Add(row);
        }

        return results;
    }

    public void InvalidateCache()
    {
        _schemaCache = null;
    }
}
