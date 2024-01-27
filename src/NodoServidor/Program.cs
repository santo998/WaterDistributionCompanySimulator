using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodoServidor;
using NodoServidor.Repositorios;
using System.Globalization;
using Utiles;
using Utiles.Telegram;

try
{
	var host = CreateHostBuilder(args).Build();

	using var scope = host.Services.CreateScope();
	var context = scope.ServiceProvider.GetRequiredService<DistribuidosObligatorioContext>();

	await context.MigrarBDAsync();

	Utilitarios.LogConsola("Conexión con la base de datos establecida");

	Servidor servidor = scope.ServiceProvider.GetRequiredService<Servidor>();

	await servidor.EjecutarAsync();
}
catch (Exception ex)
{
	Utilitarios.LogConsola(ex.ToString());
}

static IHostBuilder CreateHostBuilder(string[] args)
{
	return Host.CreateDefaultBuilder(args)
	.ConfigureAppConfiguration((__, config) =>
	{
		if (Utilitarios.EjecutandoEnDocker())
		{
			_ = config.AddJsonFile("/run/secrets/credenciales")
				.Build();
		}
		else
		{
			_ = config.SetBasePath(Directory.GetCurrentDirectory())
			.AddUserSecrets<Program>()
			.Build();
		}

		// seteamos la cultura, para definir la coma como separador decimal y el punto como separador de miles
		CultureInfo cultura = new("es-ES");

		CultureInfo.DefaultThreadCurrentCulture = cultura;
		CultureInfo.DefaultThreadCurrentUICulture = cultura;
	})
	.ConfigureLogging((logging) =>
	{
		// filtramos los mensajes de Entity Framework y System
		_ = logging.AddFilter("Microsoft", LogLevel.Warning);
		_ = logging.AddFilter("System", LogLevel.Error);
	})
	.ConfigureServices((hostContext, services) =>
	{
		var config = hostContext.Configuration;
		string? usuarioRabbit = config.GetSection("CredencialesRabbit")["Usuario"];
		string? contraseniaRabbit = config.GetSection("CredencialesRabbit")["Contrasenia"];
		string? tokenApiTelegram = config.GetSection("API_Telegram")["Token"];
		string? strConexion;

		string? nombreServidor = Utilitarios.GetVariableEntornoCadena("NOMBRE_NODO", false);
		bool persisteNodosyMediciones = Utilitarios.GetVariableEntornoBooleana("PERSISTE_NODOS_Y_MEDICIONES", false) ?? true;
		bool detectaPerdidas = Utilitarios.GetVariableEntornoBooleana("DETECTA_PERDIDAS", false) ?? true;
		string nombreHost;

		if (Utilitarios.EjecutandoEnDocker())
		{
			nombreHost = "rabbit";
			strConexion = config.GetConnectionString("DistribuidosObligatorio_Docker");
		}
		else
		{
			nombreHost = "localhost";
			strConexion = config.GetConnectionString("DistribuidosObligatorio");
		}

		if (string.IsNullOrEmpty(strConexion))
		{
			throw new ArgumentException("No se especificó el string de conexión");
		}

		if (string.IsNullOrEmpty(usuarioRabbit))
		{
			throw new ArgumentException("No se especificó el usuario Rabbit");
		}

		if (contraseniaRabbit == null)
		{
			throw new ArgumentException("No se especificó la contraseña del usuario Rabbit");
		}

		if (tokenApiTelegram == null)
		{
			throw new ArgumentException("No se especificó el token de la API Telegram");
		}

		if (string.IsNullOrEmpty(nombreServidor))
		{
			throw new ArgumentException("No se especificó el nombre del nodo");
		}

		Console.Title = nombreServidor;

		var opciones = new DbContextOptionsBuilder<DistribuidosObligatorioContext>()
			.UseSqlServer(strConexion);

		// transitorio para que cada repositorio tenga su propia instancia
		_ = services.AddTransient(_ => new DistribuidosObligatorioContext(opciones.Options));

		_ = services.AddSingleton(_ => new HttpClient());

		_ = services.AddSingleton(sp => new ClienteTelegram(tokenApiTelegram, sp.GetService<HttpClient>()));

		_ = services.AddSingleton(sp => new RepositorioNodos(sp.GetService<DistribuidosObligatorioContext>()));
		_ = services.AddSingleton(sp => new RepositorioMediciones(sp.GetService<DistribuidosObligatorioContext>()));
		_ = services.AddSingleton(sp => new RepositorioPerdidas(sp.GetService<ClienteTelegram>()
			, sp.GetService<DistribuidosObligatorioContext>()));

		_ = services.AddSingleton(sp => new Servidor(nombreServidor, nombreHost, 5672, usuarioRabbit, contraseniaRabbit, detectaPerdidas
			, persisteNodosyMediciones, sp.GetService<RepositorioPerdidas>(), sp.GetService<RepositorioNodos>()
			, sp.GetService<RepositorioMediciones>()));
	});
}