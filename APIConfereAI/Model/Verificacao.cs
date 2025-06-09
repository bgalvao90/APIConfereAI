using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIConfereAI.Model
{
    public class Verificacao
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "A URL é obrigatória")]
        [StringLength(500, ErrorMessage = "A URL não pode ter mais de 500 caracteres.")]
        [Url(ErrorMessage = "O formato da URL é inválido.")]
        public string Url { get; set; }
        [Column(TypeName = "text")]
        public string? Resultado { get; set; }
        public bool? Confiavel { get; set; }
        [StringLength(50)]
        public string? Categoria { get; set; }
        [StringLength(100)]
        public string? Dominio { get; set; }
        public string PontuacaoReclameAqui { get; set; }

        public DateTime DataHora { get; set; } = DateTime.Now;


    }
}
