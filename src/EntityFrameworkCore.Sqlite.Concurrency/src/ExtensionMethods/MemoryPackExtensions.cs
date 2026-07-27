
#if INCLUDEMEMORYPACK
using MemoryPack;

namespace EntityFrameworkCore.Sqlite.Concurrency
{
    public static class MemoryPackExtensions
    {
        public static byte[] ToMemoryPack<T>(this T obj) => MemoryPackSerializer.Serialize(obj);
        public static T FromMemoryPack<T>(this byte[] data) => MemoryPackSerializer.Deserialize<T>(data);
        
        public static async Task BulkInsertWithMemoryPack<T>(
            this DbContext context,
            IEnumerable<T> entities,
            CancellationToken ct = default) where T : class
        {
            // Store serialized entities in a separate table for fast reads
            var serialized = entities.Select(e => new SerializedEntity
            {
                Id = Guid.NewGuid(),
                TypeName = typeof(T).FullName!,
                Data = e.ToMemoryPack(),
                Created = DateTime.UtcNow
            }).ToList();
            
            await context.BulkInsertOptimizedAsync(serialized, ct);
        }
    }
    
    [MemoryPackable]
    public partial class SerializedEntity
    {
        public Guid Id { get; set; }
        public string TypeName { get; set; } = string.Empty;
        
        [MemoryPackInclude]
        public byte[] Data { get; set; } = Array.Empty<byte>();
        
        public DateTime Created { get; set; }
    }
}
#endif