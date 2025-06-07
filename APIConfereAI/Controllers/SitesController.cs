using APIConfereAI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> VerificarSite([FromBody] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest("A URL não pode ser vazia.");
            }

            try
            {
                var resultado = await _openAIService.VerificarSiteComOpenAI(url);

                var verificacao = new Verificacao
                {
                    Url = url,
                    Resultado = resultado,
                    DataHora = DateTime.UtcNow
                };

                _context.Verificacoes.Add(verificacao);
                await _context.SaveChangesAsync();

                return Ok(new { Url = url, Resultado = resultado });
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
            var lista = await _context.Verificacoes
                .OrderByDescending(v => v.DataHora)
                .ToListAsync();
            return Ok(lista);
        }

    }
}
