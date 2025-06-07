using System.ComponentModel.DataAnnotations;

namespace APIConfereAI.Model
{
    public class Verificacao
    {
        [Key]
        public int Id { get; set; }
        public string Url { get; set; }
        public string? Resultado { get; set; }
        public DateTime DataHora { get; set; } = DateTime.UtcNow;
    }
}
