using Microsoft.EntityFrameworkCore;
using Utiles;
using Utiles.Modelos;

namespace NodoServidor.Repositorios;

internal class RepositorioNodos
{
	private DistribuidosObligatorioContext _context { get; }

	public RepositorioNodos(DistribuidosObligatorioContext context)
	{
		_context = context ?? throw new ArgumentNullException(nameof(context));
	}

	public async Task<int> AltaNodoAsync(Nodo nodo)
	{
		// si el nodo padre no existe, lo damos de alta
		int resultado = await insertarNodoPadreSiNoExisteAsync(nodo);

		// si dimos de alta el nodo padre
		if (resultado > 0)
			Utilitarios.LogConsola($"Nodo padre, con Id = {nodo.Id} dado de alta parcialmente");

		// damos de alta el nodo
		return await insertarNodoAsync(nodo);
	}

	private async Task<int> insertarNodoAsync(Nodo nodo)
	{
		Nodo? nodoBd = await _context.Nodos.SingleOrDefaultAsync(n => n.Id == nodo.Id);

		// si el nodo NO estaba dado de alta
		if (nodoBd == null)
		{
			// damos de alta el nodo
			_ = await _context.Nodos.AddAsync(nodo);
		}
		else
		{
			nodoBd.IdPadre = nodo.IdPadre;
			nodoBd.Nombre = nodo.Nombre;
			nodoBd.CaudalEsperado = nodo.CaudalEsperado;
			nodoBd.ToleranciaCaudal = nodo.ToleranciaCaudal;
		}

		return await _context.SaveChangesAsync();
	}

	private async Task<int> insertarNodoPadreSiNoExisteAsync(Nodo nodo)
	{
		// si el nodo NO tiene padre
		if (nodo.IdPadre == null)
			return 0;

		Nodo? padre = await _context.Nodos.SingleOrDefaultAsync(n => n.Id == nodo.IdPadre);

		// si el padre ya está dado de alta
		if (padre != null)
			return 0;

		padre = new Nodo()
		{
			Id = nodo.IdPadre.Value
		};

		// damos de alta el padre
		_ = await _context.Nodos.AddAsync(padre);

		return await _context.SaveChangesAsync();
	}
}
