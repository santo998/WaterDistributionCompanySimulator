using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Utiles.Modelos;

public class Nodo
{
	public double CaudalEsperado { get; set; }
	public virtual ICollection<Nodo> Hijos { get; set; }
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public int Id { get; set; }
	public int? IdPadre { get; set; }
	public virtual ICollection<Medicion> Mediciones { get; set; }
	public string Nombre { get; set; } = string.Empty;
	[ForeignKey(nameof(IdPadre))]
	public virtual Nodo Padre { get; set; }
	public double ToleranciaCaudal { get; set; }

	public Nodo()
	{
	}

	public Nodo(int id, string nombre, int? padre, double caudalEsperado, double toleranciaCaudal)
	{
		Id = id;
		Nombre = nombre;
		IdPadre = padre;
		CaudalEsperado = caudalEsperado;
		ToleranciaCaudal = toleranciaCaudal;
	}
}