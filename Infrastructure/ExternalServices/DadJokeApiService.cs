using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace retoSquadmakers.Infrastructure.ExternalServices;

public interface IDadJokeApiService
{
    Task<string> GetRandomJokeAsync();
}

public class DadJokeApiService : IDadJokeApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DadJokeApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public DadJokeApiService(HttpClient httpClient, ILogger<DadJokeApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri("https://icanhazdadjoke.com/");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "RetoSquadmakers App (https://github.com/user/retoSquadmakers)");
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<string> GetRandomJokeAsync()
    {
        try
        {
            _logger.LogDebug("Fetching random Dad joke from API");
            
            var response = await _httpClient.GetAsync("");
            response.EnsureSuccessStatusCode();
            
            var jsonContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Dad Jokes API response: {Response}", jsonContent);
            
            var jokeData = JsonSerializer.Deserialize<DadJoke>(jsonContent, _jsonOptions);
            
            var joke = jokeData?.Joke ?? "Why don't scientists trust atoms? Because they make up everything!";
            
            _logger.LogDebug("Successfully retrieved Dad joke: {Joke}", joke);
            return joke;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching Dad joke: {Error}", ex.Message);
            return "Why don't scientists trust atoms? Because they make up everything!";
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while fetching Dad joke: {Error}", ex.Message);
            return "I invented a new word: Plagiarism!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching Dad joke: {Error}", ex.Message);
            return "Why did the scarecrow win an award? He was outstanding in his field!";
        }
    }
}

public class DadJoke
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("joke")]
    public string Joke { get; set; } = string.Empty;
    
    [JsonPropertyName("status")]
    public int Status { get; set; }
}