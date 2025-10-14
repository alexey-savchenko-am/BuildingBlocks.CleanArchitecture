using Polly;
using Polly.Retry;
using System.Net.Http.Json;


namespace BuildingBlocks.CleanArchitecture.Infrastructure.Http;

public abstract class RemoteService(HttpClient client, int version)
{
    private string _prefix = $"api/v{version}/";

    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy = Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    protected async Task<T?> GetAsync<T>(string url)
    {
        var response = await _retryPolicy.ExecuteAsync(() => client.GetAsync(WithVersion(url)));
        return await HandleResponse<T>(response);
    }

    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest payload)
    {
        var response = await _retryPolicy.ExecuteAsync(() => client.PostAsJsonAsync(WithVersion(url), payload));
        return await HandleResponse<TResponse>(response);
    }

    protected async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest payload)
    {
        var response = await _retryPolicy.ExecuteAsync(() => client.PutAsJsonAsync(WithVersion(url), payload));
        return await HandleResponse<TResponse>(response);
    }

    protected async Task DeleteAsync(string url)
    {
        var response = await _retryPolicy.ExecuteAsync(() => client.DeleteAsync(WithVersion(url)));
        await HandleResponse(response);
    }

    private async Task<T?> HandleResponse<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }

        await HandleError(response);

        return default;
    }

    private async Task HandleResponse(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            await HandleError(response);
        }
    }

    private async Task HandleError(HttpResponseMessage response)
    {
        var errorMessage = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException(
            $"Request failed with status code {response.StatusCode}: {errorMessage}");
    }

    private string WithVersion(string url)
        => $"{_prefix}{url}";
}
