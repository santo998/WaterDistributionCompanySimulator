using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Utiles.Modelos;

public class Perdida
{
	public bool FueNotificada { get; set; }
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }
	public int IdMedicion { get; set; }
	[ForeignKey(nameof(IdMedicion))]
	public virtual Medicion Medicion { get; set; }

	public Perdida()
	{
	}

	public Perdida(Medicion medicion)
	{
		Medicion = medicion;
	}

	public string GetDetalles()
	{
		return @$"{Medicion.FechaUTC:dd/MM/yyyy} a las {Medicion.FechaUTC:HH:mm:ss} (UTC):
En el nodo ""{Medicion.Nodo.Nombre}"" se esperaban {Medicion.Nodo.CaudalEsperado:N3} L/s de caudal, pero se midieron {Medicion.Caudal:N3} L/s con {Medicion.Nodo.ToleranciaCaudal:N3} L/s de tolerancia";
	}
}
