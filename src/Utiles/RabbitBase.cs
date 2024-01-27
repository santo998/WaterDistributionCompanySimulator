using RabbitMQ.Client;

namespace Utiles;

public abstract class RabbitBase
{
	protected string _contrasenia;
	protected string _nombreExchange = "DistribuidosObligatorio";
	protected string _nombreHost;
	protected string _nombreNodo;
	protected string _nombreQueue = "Queue";
	protected int _puerto;
	protected const string ROUTING_KEY_ALTA_NODOS = "AltaNodos";
	protected const string ROUTING_KEY_ALTA_MEDICIONES = "Mediciones";
	protected string _usuario;

	protected RabbitBase(string nombreNodo, string nombreHost, int puerto, string usuario, string contrasenia)
	{
		_nombreNodo = nombreNodo;
		_nombreHost = nombreHost;
		_puerto = puerto;
		_usuario = usuario;
		_contrasenia = contrasenia;
	}

	public virtual async Task EjecutarAsync()
	{
		using IConnection conexion = UtilesRabbit.CrearConexionRabbit(_nombreNodo, _nombreHost, _puerto, _usuario, _contrasenia);
		Utilitarios.LogConsola("Conexión con Rabbit establecida");

		using IModel canal = conexion.CreateModel();
		canal.ExchangeDeclare(_nombreExchange, ExchangeType.Direct);
		_ = canal.QueueDeclare(_nombreQueue, false, false, false, null);
		canal.QueueBind(_nombreQueue, _nombreExchange, ROUTING_KEY_ALTA_NODOS, null);
		canal.QueueBind(_nombreQueue, _nombreExchange, ROUTING_KEY_ALTA_MEDICIONES, null);

		await ejecutarAsync(canal);
	}

	protected abstract Task ejecutarAsync(IModel canal);
}
