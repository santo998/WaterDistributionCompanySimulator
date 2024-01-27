using Utiles;
using Utiles.Modelos;

namespace NodoServidor.Repositorios;

internal class RepositorioMediciones
{
	private DistribuidosObligatorioContext _context { get; }

	public RepositorioMediciones(DistribuidosObligatorioContext context)
	{
		_context = context ?? throw new ArgumentNullException(nameof(context));
	}

	public async Task<int> AltaMedicionAsync(Medicion medicion)
	{
		_ = await _context.Mediciones.AddAsync(medicion);

		// si la medición vino sin el nodo
		if (medicion.Nodo == null)
		{
			Nodo? nodo = await _context.Nodos.FindAsync(medicion.IdNodo);

			// si el nodo aún no fue dado de alta
			if (nodo == null)
			{
				// NO damos de alta la medición
				return 0;
			}

			medicion.Nodo = nodo;
		}

		return await _context.SaveChangesAsync();
	}
}
