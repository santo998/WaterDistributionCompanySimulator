using Microsoft.EntityFrameworkCore;
using Polly;
using Utiles.Modelos;

namespace Utiles;

public class DistribuidosObligatorioContext : DbContext
{
	public DbSet<Medicion> Mediciones { get; set; }
	public DbSet<Nodo> Nodos { get; set; }
	public DbSet<Perdida> Perdidas { get; set; }
	public DbSet<Suscripcion> Suscripciones { get; set; }

	public DistribuidosObligatorioContext(DbContextOptions<DistribuidosObligatorioContext> options)
		: base(options)
	{
	}

	public async Task MigrarBDAsync()
	{
		const int REINTENTOS_MAXIMOS = 10;

		// definimos los reintentos de reconexión por si el DBMS está caído
		var retryPolicy = Policy.Handle<Exception>()
			.WaitAndRetryAsync(retryCount: REINTENTOS_MAXIMOS, sleepDurationProvider: (cantidadIntentos) => TimeSpan.FromSeconds(cantidadIntentos * 2),
			onRetry: (_, duracionPausa, numeroIntento, __) => Utilitarios.LogConsola("No se pudo conectar con la base de datos." +
					$" Reintento de conexión: {numeroIntento} de {REINTENTOS_MAXIMOS} en {duracionPausa.TotalSeconds} segundos..."));

		await retryPolicy.ExecuteAsync(() => Database.MigrateAsync());
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// evitamos que las tablas tengan sus nombres en plural
		_ = modelBuilder.Entity<Nodo>().ToTable(nameof(Nodo));
		_ = modelBuilder.Entity<Medicion>().ToTable(nameof(Medicion));
		_ = modelBuilder.Entity<Perdida>().ToTable(nameof(Perdida));
		_ = modelBuilder.Entity<Suscripcion>().ToTable(nameof(Suscripcion));
	}
}
