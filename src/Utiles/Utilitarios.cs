using System.Text.Json;

namespace Utiles;

public static class Utilitarios
{
	public static T? Deserializar<T>(byte[] bytes)
	{
		var opciones = GetOpcionesJson();

		return JsonSerializer.Deserialize<T>(bytes, opciones);
	}

	public static bool EjecutandoEnDocker()
	{
		bool? ejecutandoEnDocker = GetVariableEntornoBooleana("DOTNET_RUNNING_IN_CONTAINER", false);

		return ejecutandoEnDocker ?? false;
	}

	public static double GenerarRandomico(double minimo, double maximo)
	{
		Random random = new();

		// necesario, ya que random.NextDouble() genera doubles entre 0,0 y 1,0
		return (random.NextDouble() * (maximo - minimo)) + minimo;
	}

	public static JsonSerializerOptions GetOpcionesJson()
	{
		return new JsonSerializerOptions()
		{
			PropertyNameCaseInsensitive = true,
			WriteIndented = true
		};
	}

	public static bool? GetVariableEntornoBooleana(string nombre, bool esRequerida)
	{
		string? variableEntorno = getVariableEntorno(nombre, esRequerida);

		if (variableEntorno == null)
			return null;

		if (bool.TryParse(variableEntorno, out bool valor))
			return valor;
		else
			throw new Exception($"La variable de entorno \"{nombre}\" debe tener valor booleano");
	}

	public static string? GetVariableEntornoCadena(string nombre, bool esRequerida)
	{
		return getVariableEntorno(nombre, esRequerida);
	}

	public static double? GetVariableEntornoDouble(string nombre, bool esRequerida)
	{
		string? variableEntorno = getVariableEntorno(nombre, esRequerida);

		if (variableEntorno == null)
			return null;

		if (double.TryParse(variableEntorno, out double valor))
			return valor;
		else
			throw new Exception($"La variable de entorno \"{nombre}\" debe tener valor booleano");
	}

	public static int? GetVariableEntornoEntera(string nombre, bool esRequerida)
	{
		string? variableEntorno = getVariableEntorno(nombre, esRequerida);

		if (variableEntorno == null)
			return null;

		if (int.TryParse(variableEntorno, out int valor))
			return valor;
		else
			throw new Exception($"La variable de entorno \"{nombre}\" debe tener valor booleano");
	}

	public static void LogConsola(string mensaje, DateTime fecha = default)
	{
		// DateTime.Now.ToUniversalTime() dado que como el sistema es distribuido, puede tener diferentes husos horarios
		if (fecha == default)
			fecha = DateTime.Now.ToUniversalTime();

		Console.WriteLine($"{fecha:HH:mm:ss} (UTC): {mensaje}");
	}

	public static byte[] Serializar<T>(T obj)
	{
		var opciones = GetOpcionesJson();

		return JsonSerializer.SerializeToUtf8Bytes(obj, opciones);
	}

	private static string? getVariableEntorno(string nombre, bool esRequerida)
	{
		string? variableEntorno = Environment.GetEnvironmentVariable(nombre);

		if (esRequerida && variableEntorno == null)
			throw new Exception($"Debe proporcionar la variable de entorno \"{nombre}\"");

		return variableEntorno;
	}
}
