using Utiles;

namespace NodoCliente;

internal class Medidor : IMedidorCaudal
{
	#region Implementación IMedidorCaudal
	double IMedidorCaudal.MedirCaudal(double caudalEsperado, double toleranciaCaudal)
	{
		return MedirCaudal(caudalEsperado, toleranciaCaudal);
	}
	#endregion Implementación IMedidorCaudal

	public double MedirCaudal(double caudalEsperado, double toleranciaCaudal)
	{
		// debido a que este medidor NUNCA presenta pérdidas
		double minimo = caudalEsperado - toleranciaCaudal;

		double maximo = caudalEsperado;

		return Utilitarios.GenerarRandomico(minimo, maximo);
	}
}
