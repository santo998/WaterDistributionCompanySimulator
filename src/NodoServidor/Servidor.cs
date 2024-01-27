using NodoServidor.Repositorios;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Utiles;
using Utiles.Modelos;

namespace NodoServidor;

internal class Servidor : RabbitBase
{
	private IModel? _canal;
	private bool _detectarPerdidas { get; }
	private bool _persistirNodosyMediciones { get; }
	private RepositorioMediciones _repositorioMediciones { get; }
	private RepositorioNodos _repositorioNodos { get; }
	private RepositorioPerdidas _repositorioPerdidas { get; }

	public Servidor(string nombreNodo, string nombreHost, int puerto, string usuario, string contrasenia, bool detectarPerdidas
		, bool persistirNodosyMediciones, RepositorioPerdidas repositorioPerdidas, RepositorioNodos repositorioNodos
		, RepositorioMediciones repositorioMediciones)
		: base(nombreNodo, nombreHost, puerto, usuario, contrasenia)
	{
		if (!detectarPerdidas && !persistirNodosyMediciones)
			throw new ArgumentException("No se especificó si el servidor detecta pérdidas y/o persiste mediciones, ya que el mismo debe realizar alguna tarea");

		_detectarPerdidas = detectarPerdidas;
		_persistirNodosyMediciones = persistirNodosyMediciones;
		_repositorioPerdidas = repositorioPerdidas ?? throw new ArgumentNullException(nameof(repositorioPerdidas));
		_repositorioNodos = repositorioNodos ?? throw new ArgumentNullException(nameof(repositorioNodos));
		_repositorioMediciones = repositorioMediciones ?? throw new ArgumentNullException(nameof(repositorioMediciones));
	}

	public override async Task EjecutarAsync()
	{
		if (_persistirNodosyMediciones)
			await base.EjecutarAsync();
		else if (_detectarPerdidas)
			await detectarPerdidasAsync();
	}

	protected override async Task ejecutarAsync(IModel canal)
	{
		_canal = canal;

		canal.BasicQos(0, 1, false);

		var consumidor = new EventingBasicConsumer(canal);

		consumidor.Received += consumidor_Received;

		string etiquetaConsumidor = canal.BasicConsume(_nombreQueue, false, consumidor);

		if (_detectarPerdidas)
		{
			await detectarPerdidasAsync();
		}
		else
		{
			while (Console.IsInputRedirected || !Console.KeyAvailable)
				;
		}

		canal.BasicCancel(etiquetaConsumidor);
	}

	private async Task<bool> altaMedicionAsync(byte[] cuerpoMensaje)
	{
		Medicion? medicion = Utilitarios.Deserializar<Medicion>(cuerpoMensaje);

		if (medicion == null)
		{
			Utilitarios.LogConsola("No se pudo deserializar correctamente una medición recibida");

			// procesamos el mensaje, con el objetivo de descartarlo
			return true;
		}

		int resultado = await _repositorioMediciones.AltaMedicionAsync(medicion);

		if (resultado > 0)
			Utilitarios.LogConsola($"Medición recibida de \"{medicion.Nodo.Nombre}\": {medicion.Caudal:N3} L/s efectuada a las {medicion.FechaUTC:HH:mm:ss} (UTC)");

		return true;
	}

	private async Task<bool> altaNodoAsync(byte[] cuerpoMensaje)
	{
		Nodo? nodo = Utilitarios.Deserializar<Nodo>(cuerpoMensaje);

		if (nodo == null)
		{
			Utilitarios.LogConsola("No se pudo deserializar correctamente el alta de un nodo");

			// procesamos el mensaje, con el objetivo de descartarlo
			return true;
		}

		int resultado = await _repositorioNodos.AltaNodoAsync(nodo);

		// si dimos de alta el nodo
		if (resultado > 0)
			Utilitarios.LogConsola($"Nodo \"{nodo.Nombre}\", con Id = {nodo.Id} y caudal esperado = {nodo.CaudalEsperado} L/s dado de alta");

		return true;
	}

	private async void consumidor_Received(object? sender, BasicDeliverEventArgs e)
	{
		var cuerpoMensaje = e.Body.ToArray();

		bool mensajeProcesado = await procesarMensajeAsync(cuerpoMensaje, e.RoutingKey);

		// si el mensaje fue procesado correctamente
		if (mensajeProcesado)
		{
			// le avisamos a Rabbit que procesamos el mensaje
			_canal?.BasicAck(e.DeliveryTag, false);
		}
	}

	private async Task detectarPerdidasAsync()
	{
		do
		{
			await _repositorioPerdidas.DetectarPerdidasAsync();

			// detectamos pérdidas cada diez segundos, con el fin de que se acumulen y no saturar el servidor
			await Task.Delay(10_000);
		} while (Console.IsInputRedirected || !Console.KeyAvailable);
	}

	private async Task<bool> procesarMensajeAsync(byte[] cuerpoMensaje, string routingKey)
	{
		switch (routingKey)
		{
			case ROUTING_KEY_ALTA_NODOS:
				return await altaNodoAsync(cuerpoMensaje);
			case ROUTING_KEY_ALTA_MEDICIONES:
				return await altaMedicionAsync(cuerpoMensaje);
			default:
				Utilitarios.LogConsola($"Recibido mensaje con el routing_key \"{routingKey}\", el cual no estamos manejando");
				break;
		}

		return true;
	}
}
