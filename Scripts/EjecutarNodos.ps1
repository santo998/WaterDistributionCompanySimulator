cd "C:\Proyectos\DistribuidosObligatorio\DistribuidosObligatorio"

$imagenNodoCliente = "nodocliente:dev"
$variablesEntornoCliente1 = "-e ID_NODO='1' -e NOMBRE_NODO='NodoCliente_1' -e CAUDAL_ESPERADO='100'"
$variablesEntornoCliente2 = "-e ID_NODO='2' -e NOMBRE_NODO='NodoCliente_2' -e CAUDAL_ESPERADO='60'"

docker run -v "$(pwd)/secrets.json:/run/secrets/credenciales:ro" $variablesEntornoCliente1 $imagenNodoCliente
docker run -v "$(pwd)/secrets.json:/run/secrets/credenciales:ro" $variablesEntornoCliente2 $imagenNodoCliente

Pause