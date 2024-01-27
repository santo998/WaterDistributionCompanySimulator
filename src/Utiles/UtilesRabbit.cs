using Polly;
using RabbitMQ.Client;

namespace Utiles;

public static class UtilesRabbit
{
	public static IConnection CrearConexionRabbit(string nombreNodo, string nombreHost, int puerto, string usuario, string contrasenia)
	{
		var factory = new ConnectionFactory
		{
			ClientProvidedName = nombreNodo,
			HostName = nombreHost,
			UserName = usuario,
			Password = contrasenia,
			VirtualHost = "/",
			Port = puerto
		};

		const int REINTENTOS_MAXIMOS = 10;

		// definimos los reintentos de reconexión por si Rabbit está caído
		var retryPolicy = Policy.Handle<Exception>()
			.WaitAndRetry(retryCount: REINTENTOS_MAXIMOS, sleepDurationProvider: (cantidadIntentos) => TimeSpan.FromSeconds(cantidadIntentos * 2),
			onRetry: (_, duracionPausa, numeroIntento, __) => Utilitarios.LogConsola("No se pudo conectar con Rabbit." +
					$" Reintento de conexión: {numeroIntento} de {REINTENTOS_MAXIMOS} en {duracionPausa.TotalSeconds} segundos..."));

		return retryPolicy.Execute(() => factory.CreateConnection());
	}
}