 
#if INCLUDESPECTRE
using Spectre.Console;
using Microsoft.Data.Sqlite;

namespace EntityFrameworkCore.Sqlite.Concurrency
{
    public static class SqliteDiagnostics
    {
        public static void ShowConcurrencyStats(DbContext context)
        {
            var table = new Table();
            table.Title = new TableTitle("SQLite Thread Safety Diagnostics");
            
            table.AddColumn("Metric");
            table.AddColumn("Value");
            
            try
            {
                var connection = context.Database.GetDbConnection();
                using var cmd = connection.CreateCommand();
                
                cmd.CommandText = @"
                    PRAGMA journal_mode;
                    PRAGMA synchronous;
                    PRAGMA wal_checkpoint(TRUNCATE);
                    SELECT COUNT(*) FROM sqlite_master;
                    PRAGMA page_count;
                    PRAGMA freelist_count;
                ";
                
                using var reader = cmd.ExecuteReader();
                var results = new List<string>();
                while (reader.Read())
                {
                    results.Add(reader.GetString(0));
                }
                
                table.AddRow("Journal Mode", results.Count > 0 ? results[0] : "N/A");
                table.AddRow("Synchronous Mode", results.Count > 1 ? results[1] : "N/A");
                table.AddRow("Last Checkpoint", results.Count > 2 ? results[2] : "N/A");
                table.AddRow("Table Count", results.Count > 3 ? results[3] : "N/A");
                table.AddRow("Total Pages", results.Count > 4 ? results[4] : "N/A");
                table.AddRow("Free Pages", results.Count > 5 ? results[5] : "N/A");
                
                // Calculate fragmentation
                if (results.Count > 4 && results.Count > 5 && 
                    int.TryParse(results[4], out var total) && 
                    int.TryParse(results[5], out var free) && total > 0)
                {
                    var fragmentation = (free * 100.0 / total).ToString("F2") + "%";
                    table.AddRow("Fragmentation", fragmentation);
                }
            }
            catch (Exception ex)
            {
                table.AddRow("Error", ex.Message);
            }
            
            AnsiConsole.Write(table);
        }
        
        public static async Task MonitorPerformanceAsync(
            DbContext context, 
            TimeSpan duration, 
            CancellationToken ct = default)
        {
            var endTime = DateTime.UtcNow.Add(duration);
            var stats = new List<PerformanceStat>();
            
            while (DateTime.UtcNow < endTime && !ct.IsCancellationRequested)
            {
                var stat = await GetCurrentStatsAsync(context);
                stats.Add(stat);
                
                await Task.Delay(TimeSpan.FromSeconds(1), ct);
            }
            
            DisplayPerformanceChart(stats);
        }
        
        private static async Task<PerformanceStat> GetCurrentStatsAsync(DbContext context)
        {
            var connection = context.Database.GetDbConnection();
            using var cmd = connection.CreateCommand();
            
            cmd.CommandText = @"
                PRAGMA wal_checkpoint;
                SELECT changes(), total_changes();
            ";
            
            using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            
            return new PerformanceStat
            {
                Timestamp = DateTime.UtcNow,
                CheckpointResult = reader.GetString(0),
                RecentChanges = reader.GetInt32(1),
                TotalChanges = reader.GetInt32(2)
            };
        }
        
        private static void DisplayPerformanceChart(List<PerformanceStat> stats)
        {
            var chart = new BarChart()
                .Width(60)
                .Label("[green bold underline]Performance Metrics[/]")
                .CenterLabel();
            
            if (stats.Any())
            {
                chart.AddItem("Avg Changes/Sec", 
                    stats.Average(s => s.RecentChanges), 
                    Color.Green);
                chart.AddItem("Total Changes", 
                    stats.Last().TotalChanges, 
                    Color.Blue);
            }
            
            AnsiConsole.Write(chart);
        }
        
        private class PerformanceStat
        {
            public DateTime Timestamp { get; set; }
            public string CheckpointResult { get; set; } = string.Empty;
            public int RecentChanges { get; set; }
            public int TotalChanges { get; set; }
        }
    }
}
#endif