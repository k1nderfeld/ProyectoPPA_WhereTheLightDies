using UnityEngine;

// Escopeta: cada disparo lanza varios perdigones con dispersión (RF-14).
// Usa cartuchos (se recargan de a uno hasta llenar) y ocupa 2 slots del inventario.
// Clase "Escopeta" del diagrama de clases. Se agrega al objeto de la escopeta.
public class Escopeta : Arma
{
    [Header("Escopeta")]
    public float dispersion = 6f;   // grados que se abre cada perdigón respecto del centro

    public override void ValoresIniciales()
    {
        danioPorRayo = 8f;
        rango = 15f;
        cantidadRayosAtaque = 8;
        cadencia = 1f;
        tiempoRecarga = 2.5f;
        municionPorCarga = 6;
        municionActual = 6;
        slotsInventario = 2;
        tipoMunicion = TipoMunicion.Cartucho;
        posicionEnMano = new Vector3(0.22f, -0.24f, 0.55f);
        rotacionEnMano = new Vector3(0f, -3f, 0f);
    }

    // Cada perdigón sale un poco desviado, al azar, dentro del ángulo de dispersión
    protected override Vector3 DireccionDelRayo(Transform camara)
    {
        Quaternion desvio = Quaternion.Euler(Random.Range(-dispersion, dispersion), Random.Range(-dispersion, dispersion), 0f);
        return camara.rotation * desvio * Vector3.forward;
    }
}
