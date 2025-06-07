using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class OpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public OpenAIService(IConfiguration configuration)
    {
        _httpClient = new HttpClient();

        _apiKey = configuration["Groq:ApiKey"] ?? Environment.GetEnvironmentVariable("GROQ_API_KEY");

        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new Exception("A chave da Groq está vazia ou não foi encontrada.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<string> VerificarSiteComOpenAI(string url)
    {
        var prompt = $"Esse site parece perigoso ou suspeito? Analise com base apenas na URL e de algums fontes confiaveis que você irá buscar na internet e diga se ele pode ser um site malicioso ou confiável, somente me Responda com true para confiável ou false para suspeito, responda somente com True para confiavel e False para Malicioso e me diga a reputação do site baseado no ReclameAqui.com.br: {url}";

        var request = new
        {
            model = "meta-llama/llama-4-scout-17b-16e-instruct",
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Erro da Groq: {response.StatusCode} - {responseJson}");
        }

        using JsonDocument doc = JsonDocument.Parse(responseJson);
        string resposta = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return resposta?.Trim() ?? "Resposta vazia da OpenAI.";
    }
}
