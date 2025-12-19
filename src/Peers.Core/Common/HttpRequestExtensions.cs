namespace Peers.Core.Common;

/// <summary>
/// Provides extension methods for working with <see cref="HttpRequest"/> instances.
/// </summary>
public static class HttpRequestExtensions
{
    extension(HttpRequest req)
    {
        /// <summary>
        /// Retrieves the value of the specified query string parameter from the current request.
        /// </summary>
        /// <param name="key">The name of the query string parameter to retrieve. Cannot be null.</param>
        /// <returns>The value of the query string parameter if it exists; otherwise, null.</returns>
        public string? GetQueryValue(string key)
            => req.Query.TryGetValue(key, out var value) ? value.ToString() : null;
    }
}
