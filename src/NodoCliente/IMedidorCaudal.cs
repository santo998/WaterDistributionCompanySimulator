namespace NodoCliente;

internal interface IMedidorCaudal
{
	double MedirCaudal(double caudalEsperado, double toleranciaCaudal);
}