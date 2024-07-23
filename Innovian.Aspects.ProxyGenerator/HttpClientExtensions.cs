using System.Net.Http.Json;
using System.Text.Json;

namespace Innovian.Aspects.ProxyGenerator;

/// <summary>
/// Extension methods for an <see cref="HttpClient"/> instance.
/// </summary>
public static class HttpExtensions
{
    /// <summary>
    /// Gets a response from an API endpoint and deserializes the result.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to.</typeparam>
    /// <param name="httpClient">The HttpClient instance.</param>
    /// <param name="requestUri">The URI to make the request to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized result from the request.</returns>
    /// <exception cref="Exception"></exception>
    public static async Task<T> GetAsAsync<T>(this HttpClient httpClient, string requestUri, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStreamAsync(cancellationToken);
            var deserializedResult = await JsonSerializer.DeserializeAsync<T>(result, cancellationToken: cancellationToken);
            if (deserializedResult is not null)
            {
                return deserializedResult;
            }
        }

        throw new Exception("Unable to retrieve and deserialize the response from the API endpoint");
    }

    /// <summary>
    /// Posts a body to an API endpoint and deserializes the result.
    /// </summary>
    /// <typeparam name="TRequest">The type of the payload being sent as JSON in the request.</typeparam>
    /// <typeparam name="TResponse">The type of the payload to deserialize the response to.</typeparam>
    /// <param name="httpClient">The HttpClient instance.</param>
    /// <param name="requestUri">The URI to make the request to.</param>
    /// <param name="payload">The payload provided to the endpoint.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized result from the request.</returns>
    public static async Task<TResponse> PostAsAsync<TRequest, TResponse>(this HttpClient httpClient, string requestUri, TRequest payload,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(requestUri, payload, cancellationToken: cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStreamAsync(cancellationToken);
            var deserializedResult =
                await JsonSerializer.DeserializeAsync<TResponse>(result, cancellationToken: cancellationToken);

            if (deserializedResult is not null)
            {
                return deserializedResult;
            }
        }

        throw new Exception("Unable to retrieve and deserialize response from the API endpoint");
    }
}