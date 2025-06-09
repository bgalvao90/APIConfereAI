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
        var prompt = $@"
Você é um verificador especializado em segurança e reputação de sites na internet.

Analise a URL abaixo e responda APENAS com um JSON contendo os seguintes campos:

- ""Confiavel"": true ou false (verdadeiro se o domínio for legítimo, profissional, seguro e com boa reputação),
- ""Categoria"": categoria principal do site (ex: Streaming, E-commerce, Banco, Governo, Notícias, Jogos),
- ""Dominio"": domínio principal da URL (ex: max.com, gov.br, mercado livre),
- ""PontuacaoReclameAqui"": esse campo deve ser string, nota estimada de 0 a 10 baseada na reputação pública (site reclameaqui.com.br, siteconfiavel.com.br, e outras fontes confiáveis),
- ""Resultado"": breve resumo da análise (máximo 3 frases).

Regras importantes:

- Considere como confiável sites legítimos que possuam protocolo HTTPS, aparência profissional, e ausência de reclamações graves, mesmo que sejam pouco conhecidos ou novos.
- Se não houver informações suficientes sobre a reputação do site, atribua uma pontuação intermediária (ex: 5) e não marque automaticamente como não confiável.
- Apenas atribua ""confiavel"": false se houver indícios claros de risco, fraude, má reputação ou reclamações significativas.
- Domínios novos ou incomuns não devem ser automaticamente marcados como suspeitos.
- Utilize múltiplas fontes de reputação para a análise, como sites de reclamação e avaliações públicas.
- Retorne somente um JSON puro, sem texto extra ou comentários.
- Se não houver cadastro no ReclameAqui.com.br, utilize outras fontes confiáveis para estimar a pontuação de reputação ou retorne como não cadastrado.

URL a ser analisada: {url}

";



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
