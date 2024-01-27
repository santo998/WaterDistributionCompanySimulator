using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodoCliente;
using System.Globalization;
using Utiles;
using Utiles.Modelos;

try
{
	var host = createHostBuilder(args).Build();

	using var scope = host.Services.CreateScope();
	Cliente cliente = scope.ServiceProvider.GetRequiredService<Cliente>();

	await cliente.EjecutarAsync();
}
catch (Exception ex)
{
	Utilitarios.LogConsola(ex.ToString());
}

static IHostBuilder createHostBuilder(string[] args) =>
	Host.CreateDefaultBuilder(args)
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
	.ConfigureServices((hostContext, services) =>
	{
		var config = hostContext.Configuration;
		string? usuarioRabbit = config.GetSection("CredencialesRabbit")["Usuario"];
		string? contraseniaRabbit = config.GetSection("CredencialesRabbit")["Contrasenia"];

		int? id = Utilitarios.GetVariableEntornoEntera("ID_NODO", true);
		string? nombreNodo = Utilitarios.GetVariableEntornoCadena("NOMBRE_NODO", true);
		int? padre = Utilitarios.GetVariableEntornoEntera("PADRE_NODO", false);
		double? caudalEsperado = Utilitarios.GetVariableEntornoDouble("CAUDAL_ESPERADO", true);

		// si no nos pasaron la tolerancia a las mediciones, la seteamos en 0,005 L/s
		double toleranciaMediciones = Utilitarios.GetVariableEntornoDouble("TOLERANCIA_MEDICIONES", false) ?? 0.005;

		// si no nos especificaron si el nodo tiene pérdidas, suponemos que no tiene
		bool conPerdidas = Utilitarios.GetVariableEntornoBooleana("CON_PERDIDAS", false) ?? false;
		string nombreHost;

		if (string.IsNullOrEmpty(usuarioRabbit))
		{
			throw new ArgumentException("No se especificó el usuario Rabbit");
		}

		if (contraseniaRabbit == null)
		{
			throw new ArgumentException("No se especificó la contraseña del usuario Rabbit");
		}

		if (id == null)
		{
			throw new ArgumentException("No se especificó el id del nodo");
		}

		if (string.IsNullOrEmpty(nombreNodo))
		{
			throw new ArgumentException("No se especificó el nombre del nodo");
		}

		if (caudalEsperado == null)
		{
			throw new ArgumentException("No se especificó el caudal esperado del nodo");
		}

		if (caudalEsperado <= 0)
		{
			throw new ArgumentException("El caudal esperado del nodo debe ser mayor a cero");
		}

		Console.Title = nombreNodo;

		if (Utilitarios.EjecutandoEnDocker())
		{
			nombreHost = "rabbit";
		}
		else
		{
			nombreHost = "localhost";
		}

		if (conPerdidas)
		{
			_ = services.AddSingleton<IMedidorCaudal, MedidorConPerdidas>();
		}
		else
		{
			_ = services.AddSingleton<IMedidorCaudal, Medidor>();
		}

		Nodo nodo = new(id.Value, nombreNodo, padre, caudalEsperado.Value, toleranciaMediciones);

		_ = services.AddSingleton(sp => new Cliente(nodo, sp.GetService<IMedidorCaudal>(), nombreHost, 5672
			, usuarioRabbit, contraseniaRabbit));
	});
