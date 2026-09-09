namespace Reaparr.FluentResults
{
    /// <summary>
    /// Extensions for <see cref="IReason"/>
    /// </summary>
    public static class ReasonExtensions
    {
        /// <summary>
        /// Check if a metadata key exists
        /// </summary>
        /// <param name="reason">The reason instance</param>
        /// <param name="key">The metadata key</param>
        /// <returns>True if the metadata key exists</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool HasMetadataKey(this IReason reason, string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return reason.Metadata.ContainsKey(key);
        }
    }
}
