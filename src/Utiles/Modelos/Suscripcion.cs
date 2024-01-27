using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Utiles.Modelos;

public class Suscripcion
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public string ChatId { get; set; }

	public Suscripcion()
	{
	}

	public Suscripcion(string chatId)
	{
		ChatId = chatId;
	}
}
