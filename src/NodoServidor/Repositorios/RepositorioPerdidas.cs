using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Utiles;
using Utiles.Modelos;
using Utiles.Telegram;

namespace NodoServidor.Repositorios;

internal class RepositorioPerdidas
{
	private DistribuidosObligatorioContext _context { get; }
	private ClienteTelegram _telegram { get; }

	public RepositorioPerdidas(ClienteTelegram telegram, DistribuidosObligatorioContext context)
	{
		_telegram = telegram ?? throw new ArgumentNullException(nameof(telegram));
		_context = context ?? throw new ArgumentNullException(nameof(context));
	}

	public async Task DetectarPerdidasAsync()
	{
		const int MAX_PERDIDAS = 25;

		// obtenemos las mediciones que NO fueron analizadas (es decir, a las que no se les buscó pérdidas)
		// , o que NO tengan un objeto pérdida asociado, o su pérdida asociada no haya sido notificada
		// , y que efectivamente presenten pérdidas
		List<Medicion> medicionesConPerdidasSinAnalizarONotificar = await _context.Mediciones
			.Select(m => m)
			.Include(m => m.Nodo)
			.Include(m => m.Perdida)
			.Where(m => !m.Analizada || m.Perdida == null || !m.Perdida.FueNotificada)
			.Where(m => m.Caudal <= (m.Nodo.CaudalEsperado - m.Nodo.ToleranciaCaudal - double.Epsilon))
			.OrderBy(m => m.Id)
			.Take(MAX_PERDIDAS)
			.ToListAsync();

		// si NO hay mediciones sin analizar, o pérdidas no notificadas y que efectivamente presentan pérdidas
		if (medicionesConPerdidasSinAnalizarONotificar.IsNullOrEmpty())
			return;

		List<Perdida> perdidas = await altaPerdidasAsync(medicionesConPerdidasSinAnalizarONotificar);

		try
		{
			await notificarAsync(perdidas);
		}
		catch (Exception ex)
		{
			Utilitarios.LogConsola($"Bot Telegram: error al notificar: {ex}");
		}
	}

	private async Task<List<string>> actualizarSuscripcionesAsync()
	{
		List<Suscripcion>? suscripciones = await _context.Suscripciones.ToListAsync();
		List<string>? chatIdsNuevos = await _telegram.GetChatsIdsNuevosAsync();
		List<string> chatIds = [];

		// agregamos los chatId de las suscripciones existentes
		foreach (var suscripcion in suscripciones)
			chatIds.Add(suscripcion.ChatId);

		// si hubieron nuevas suscripciones
		if (chatIdsNuevos != null)
		{
			// obtenemos los chatIds faltantes
			var idsChatsFaltantes = chatIdsNuevos.Except(chatIds);

			// agregamos los chatsIds faltantes a la lista principal
			chatIds.AddRange(idsChatsFaltantes);

			// las damos de alta en la BD
			foreach (var chatId in chatIdsNuevos)
			{
				// si la suscripción ya existe
				if (await _context.Suscripciones.AnyAsync(s => s.ChatId == chatId))
					continue;

				Suscripcion suscripcion = new(chatId);

				_ = await _context.AddAsync(suscripcion);

				_ = await _context.SaveChangesAsync();
			}
		}

		return chatIds;
	}

	private async Task<List<Perdida>> altaPerdidasAsync(List<Medicion> medicionesConPerdidasSinAnalizar)
	{
		List<Perdida> perdidas = [];

		foreach (var medicion in medicionesConPerdidasSinAnalizar)
		{
			if (medicion.Perdida == null)
			{
				medicion.Perdida = new Perdida(medicion);

				_ = await _context.Perdidas.AddAsync(medicion.Perdida);
			}

			perdidas.Add(medicion.Perdida);

			medicion.Analizada = true;

			_ = _context.Mediciones.Update(medicion);
			_ = await _context.SaveChangesAsync();
		}

		return perdidas;
	}

	private async Task notificarAsync(List<Perdida> perdidas)
	{
		if (perdidas.IsNullOrEmpty())
			throw new ArgumentNullException(nameof(perdidas));

		List<string> chatIds = await actualizarSuscripcionesAsync();

		if (chatIds.IsNullOrEmpty())
		{
			Utilitarios.LogConsola("No se obtuvieron los ids de los chats. Debe iniciar al menos un chat con el bot antes de poder recibir notificaciones de este");

			return;
		}

		string detalles = string.Join(Environment.NewLine + Environment.NewLine, perdidas.Select(p => p.GetDetalles()));
		string mensaje = $"Pérdida/s detectada/s:{Environment.NewLine}{detalles}";

		await _telegram.EnviarBroadcastAsync(mensaje, chatIds);

		foreach (var perdida in perdidas)
			perdida.FueNotificada = true;

		_context.Perdidas.UpdateRange(perdidas);
		_ = await _context.SaveChangesAsync();

		Utilitarios.LogConsola($"Pérdida/s notificada/s:{Environment.NewLine}{detalles}");
	}
}
