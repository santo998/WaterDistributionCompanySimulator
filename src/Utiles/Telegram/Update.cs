namespace Utiles.Telegram;

internal class Update
{
	public long Update_id { get; set; }
	public Message? Message { get; set; }

	public string? GetIdChat()
	{
		return Message?.Chat?.Id.ToString();
	}
}
