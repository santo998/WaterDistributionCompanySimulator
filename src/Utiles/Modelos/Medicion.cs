using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Utiles.Modelos;

public class Medicion
{
	// indica si la medición fue analizada en búsqueda de pérdidas
	public bool Analizada { get; set; }
	public double Caudal { get; set; }
	// guardamos la fecha UTC, dado que como el sistema es distribuido, puede tener diferentes husos horarios
	public DateTime FechaUTC { get; set; }
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }
	public int IdNodo { get; set; }
	[ForeignKey(nameof(IdNodo))]
	public virtual Nodo Nodo { get; set; }
	public virtual Perdida Perdida { get; set; }

	public Medicion()
	{
	}

	public Medicion(double caudal, DateTime fecha, int idNodo)
	{
		Caudal = caudal;
		FechaUTC = fecha;
		IdNodo = idNodo;
	}

	public bool PresentaPerdidas()
	{
		double maximoCaudalConPerdidas = Nodo.CaudalEsperado - Nodo.ToleranciaCaudal - double.Epsilon;

		return Caudal <= maximoCaudalConPerdidas;
	}
}
