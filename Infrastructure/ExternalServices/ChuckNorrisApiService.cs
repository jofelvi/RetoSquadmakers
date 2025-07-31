using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace retoSquadmakers.Infrastructure.ExternalServices;

public interface IChuckNorrisApiService
{
    Task<string> GetRandomJokeAsync();
}

public class ChuckNorrisApiService : IChuckNorrisApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ChuckNorrisApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ChuckNorrisApiService(HttpClient httpClient, ILogger<ChuckNorrisApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri("https://api.chucknorris.io/");
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
    }

    public async Task<string> GetRandomJokeAsync()
    {
        try
        {
            _logger.LogDebug("Fetching random Chuck Norris joke from API");
            
            var response = await _httpClient.GetAsync("jokes/random");
            response.EnsureSuccessStatusCode();
            
            var jsonContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Chuck Norris API response: {Response}", jsonContent);
            
            var jokeData = JsonSerializer.Deserialize<ChuckNorrisJoke>(jsonContent, _jsonOptions);
            
            var joke = jokeData?.Value ?? "Chuck Norris doesn't need jokes, jokes need Chuck Norris.";
            
            _logger.LogDebug("Successfully retrieved Chuck Norris joke: {Joke}", joke);
            return joke;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching Chuck Norris joke: {Error}", ex.Message);
            return "Chuck Norris doesn't need the internet, the internet needs Chuck Norris.";
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while fetching Chuck Norris joke: {Error}", ex.Message);
            return "Chuck Norris counted to infinity. Twice.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching Chuck Norris joke: {Error}", ex.Message);
            return "Chuck Norris can handle any exception, even this one.";
        }
    }
}

public class ChuckNorrisJoke
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}