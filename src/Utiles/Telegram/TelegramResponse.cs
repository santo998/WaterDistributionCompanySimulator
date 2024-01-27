namespace Utiles.Telegram;

internal class TelegramResponse
{
	public bool Ok { get; set; }
	public List<Update>? Result { get; set; }

	public List<string> GetChatsIds()
	{
		List<string> ids = [];

		if (Result == null)
			return ids;

		foreach (var update in Result)
		{
			string? id = update.GetIdChat();

			if (id != null)
				ids.Add(id);
		}

		return ids;
	}
}
