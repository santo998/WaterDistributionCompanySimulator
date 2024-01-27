namespace Utiles.Telegram;

internal class Message
{
	public Chat? Chat { get; set; }
	public long Date { get; set; }
	public From? From { get; set; }
	public long Message_id { get; set; }
	public string? Text { get; set; }
}
