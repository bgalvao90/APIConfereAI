using APIConfereAI.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Verificacao> Verificacoes { get; set; }
}
