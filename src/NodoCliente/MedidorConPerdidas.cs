using Utiles;

namespace NodoCliente;

internal class MedidorConPerdidas : IMedidorCaudal
{
	#region Implementación IMedidorCaudal
	double IMedidorCaudal.MedirCaudal(double caudalEsperado, double toleranciaCaudal)
	{
		return MedirCaudal(caudalEsperado, toleranciaCaudal);
	}
	#endregion Implementación IMedidorCaudal

	public double MedirCaudal(double caudalEsperado, double toleranciaCaudal)
	{
		// debido a que este medidor SIEMPRE presenta pérdidas
		double minimo = 0;

		// debido a que este medidor SIEMPRE presenta pérdidas
		double maximo = caudalEsperado - toleranciaCaudal - double.Epsilon;

		return Utilitarios.GenerarRandomico(minimo, maximo);
	}
}
