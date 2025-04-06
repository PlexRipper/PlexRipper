namespace PlexRipper.Domain;

public static class HttpRequestMessageExtensions
{
    public static IDictionary<string, string> ParseQueryToDictionary(this HttpRequestMessage request)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (request.RequestUri == null)
            return dict;

        // Get the query string (e.g. "?param=value&foo=bar")
        var query = request.RequestUri.Query;
        if (string.IsNullOrWhiteSpace(query))
            return dict;

        // Remove the leading '?' if present
        if (query.StartsWith("?"))
            query = query.Substring(1);

        // Split the query string by '&'
        var pairs = query.Split(['&'], StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in pairs)
        {
            // Split each pair by '='. Use a count of 2 so that values containing '=' are preserved.
            var parts = pair.Split(['='], 2);
            if (parts.Length >= 1)
            {
                // Decode key and value
                var key = Uri.UnescapeDataString(parts[0]);
                var value = parts.Length == 2 ? Uri.UnescapeDataString(parts[1]) : string.Empty;

                // If the key already exists, you might want to decide how to handle duplicates.
                // For now, we'll keep the first occurrence.
                if (!dict.ContainsKey(key))
                {
                    dict.Add(key, value);
                }
            }
        }

        return dict;
    }
}
