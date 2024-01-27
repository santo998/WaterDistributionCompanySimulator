using RabbitMQ.Client;
using Utiles;
using Utiles.Modelos;

namespace NodoCliente;

internal class Cliente : RabbitBase
{
	private IMedidorCaudal _medidor { get; }
	private Nodo _nodo { get; }

	public Cliente(Nodo nodo, IMedidorCaudal medidor, string nombreHost, int puerto, string usuario, string contrasenia)
		: base(nodo.Nombre, nombreHost, puerto, usuario, contrasenia)
	{
		ArgumentNullException.ThrowIfNull(medidor);

		_nodo = nodo;
		_medidor = medidor;
	}

	protected override async Task ejecutarAsync(IModel canal)
	{
		Utilitarios.LogConsola("Dando de alta el nodo...");

		byte[] cuerpoMensajeBytes = Utilitarios.Serializar(_nodo);
		canal.BasicPublish(_nombreExchange, ROUTING_KEY_ALTA_NODOS, null, cuerpoMensajeBytes);

		do
		{
			double caudalMedido = _medidor.MedirCaudal(_nodo.CaudalEsperado, _nodo.ToleranciaCaudal);

			// DateTime.Now.ToUniversalTime() dado que como el sistema es distribuido, puede tener diferentes husos horarios
			Medicion medicion = new(caudalMedido, DateTime.Now.ToUniversalTime(), _nodo.Id);

			Utilitarios.LogConsola($"Caudal medido: {caudalMedido:N3} L/s", medicion.FechaUTC);

			cuerpoMensajeBytes = Utilitarios.Serializar(medicion);
			canal.BasicPublish(_nombreExchange, ROUTING_KEY_ALTA_MEDICIONES, null, cuerpoMensajeBytes);

			// tomamos mediciones cada segundo, con el fin de hacer las mínimas necesarias y no saturar el servidor
			await Task.Delay(1_000);
		} while (Console.IsInputRedirected || !Console.KeyAvailable);
	}
}
