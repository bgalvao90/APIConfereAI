using APIConfereAI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace APIConfereAI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SitesController : ControllerBase
    {
        private readonly OpenAIService _openAIService;
        private readonly AppDbContext _context;

        public SitesController(OpenAIService openAIService, AppDbContext context)
        {
            _openAIService = openAIService;
            _context = context;
        }
        [HttpPost("verificar")]
        public async Task<IActionResult> VerificarSite([FromBody] UrlRequest request)
        {
            string url = request?.Url;
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest("A URL não pode ser vazia.");
            }

            string dominio;
            try
            {
                var uri = new UriBuilder(url).Uri;
                dominio = uri.Host.ToLower();
                if (dominio.StartsWith("www.")) dominio = dominio.Substring(4);
            }
            catch (UriFormatException)
            {
                return BadRequest("Formato de URL inválido.");
            }

            // Verifica se já existe no banco esse domínio, independente da data
            var verificacaoExistente = await _context.Verificacoes
                .FirstOrDefaultAsync(v => v.Dominio.ToLower() == dominio);

            if (verificacaoExistente != null)
            {
                return Ok(verificacaoExistente);
            }

            try
            {
                var resultadoJson = await _openAIService.VerificarSiteComOpenAI(url);

                var jsonLimpo = resultadoJson
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();

                RespostaOpenAI resposta;
                try
                {
                    resposta = JsonSerializer.Deserialize<RespostaOpenAI>(jsonLimpo, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro ao desserializar JSON: " + ex.Message);
                    Console.WriteLine("Conteúdo retornado: " + resultadoJson);
                    throw new Exception("A resposta da IA não está em formato JSON válido.");
                }

                var verificacao = new Verificacao
                {
                    Url = url,
                    Resultado = resposta.Resultado,
                    DataHora = DateTime.Now,
                    Confiavel = resposta.Confiavel,
                    Categoria = string.IsNullOrWhiteSpace(resposta.Categoria) ? "Não especificada" : resposta.Categoria,
                    Dominio = string.IsNullOrWhiteSpace(resposta.Dominio)
                        ? dominio
                        : resposta.Dominio.ToLower().Replace("www.", ""),
                    PontuacaoReclameAqui = string.IsNullOrWhiteSpace(resposta.PontuacaoReclameAqui)
                        ? "Não cadastrado"
                        : resposta.PontuacaoReclameAqui
                };

                _context.Verificacoes.Add(verificacao);
                await _context.SaveChangesAsync();

                return Ok(verificacao);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Erro interno: " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao verificar o site: {ex.Message}");
            }
        }

        public class UrlRequest
        {
            public string Url { get; set; }
        }

        [HttpGet("historico")]
        public async Task<IActionResult> ObterHistorico()
        {
            if (_context.Verificacoes == null)
            {
                return NotFound("Nenhuma verificação encontrada.");
            }
            var lista = await _context.Verificacoes
                .OrderByDescending(v => v.DataHora)
                .ToListAsync();
            return Ok(lista);
        }

    }
}
