using System.Text.Json;

namespace Utiles.Telegram;

public class ClienteTelegram
{
	private string _apiToken { get; }
	private HttpClient _clienteHttp { get; }
	private string _urlEnviarMensajes { get; }
	private string _urlGetMensajes { get; }

	public ClienteTelegram(string apiToken, HttpClient clienteHttp)
	{
		_apiToken = apiToken;
		_clienteHttp = clienteHttp ?? throw new ArgumentNullException(nameof(clienteHttp));

		_urlEnviarMensajes = $"https://api.telegram.org/bot{_apiToken}/sendMessage";
		_urlGetMensajes = $"https://api.telegram.org/bot{_apiToken}/getUpdates";
	}

	public async Task EnviarBroadcastAsync(string mensaje, List<string> idsChats)
	{
		try
		{
			List<Task> enviosMensajes = [];

			foreach (string chatId in idsChats)
			{
				Task envioMensaje = enviarMensajeAsync(mensaje, chatId);

				enviosMensajes.Add(envioMensaje);
			}

			// esperamos a que se envíen todos los mensajes
			await Task.WhenAll(enviosMensajes);
		}
		catch (Exception ex)
		{
			Utilitarios.LogConsola($"Ocurrió un error al obtener los ids de los chats: {ex.Message}");

			throw;
		}
	}

	public async Task<List<string>?> GetChatsIdsNuevosAsync()
	{
		string respuesta = await _clienteHttp.GetStringAsync(_urlGetMensajes);
		var opciones = Utilitarios.GetOpcionesJson();

		TelegramResponse? telegramResponse = JsonSerializer.Deserialize<TelegramResponse>(respuesta, opciones);

		return telegramResponse?.GetChatsIds();
	}

	private async Task enviarMensajeAsync(string mensaje, string chatId)
	{
		try
		{
			var parametros = new Dictionary<string, string>
			{
				{ "chat_id", chatId },
				{ "text", mensaje }
			};

			var respuesta = await _clienteHttp.PostAsync(_urlEnviarMensajes, new FormUrlEncodedContent(parametros));

			if (!respuesta.IsSuccessStatusCode)
			{
				Utilitarios.LogConsola($"Error al enviar el mensaje al chat ID {chatId}. Estado de respuesta: {respuesta.StatusCode}");

				return;
			}

			Utilitarios.LogConsola($"Mensaje enviado al chat con ID \"{chatId}\"");
		}
		catch (Exception ex)
		{
			Utilitarios.LogConsola($"Ocurrió un error al enviar el mensaje al chat con ID \"{chatId}\": {ex.Message}");
		}
	}
}