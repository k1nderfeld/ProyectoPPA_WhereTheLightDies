using UnityEngine;

// Arma cuerpo a cuerpo (la palanca): golpea a lo que esté a 2 m o menos, sin usar munición (RF-12).
// Clase "CuerpoACuerpo" del diagrama de clases. Se agrega al objeto de la palanca.
public class CuerpoACuerpo : Arma
{
    public override void ValoresIniciales()
    {
        danioPorRayo = 25f;
        rango = 2f;
        cantidadRayosAtaque = 1;
        cadencia = 1.5f;
        tiempoRecarga = 0f;
        municionPorCarga = 0;   // 0 = no usa munición
        municionActual = 0;
        slotsInventario = 1;
        posicionEnMano = new Vector3(0.25f, -0.25f, 0.5f);
        rotacionEnMano = new Vector3(10f, -10f, 0f);
    }

    // La palanca no tiene retroceso: se levanta hacia un lado y vuelve bajando, como un golpe en arco
    protected override void Retroceso()
    {
        transform.localPosition = posicionEnMano + new Vector3(-0.1f, 0.05f, 0.05f);
        transform.localRotation = Quaternion.Euler(rotacionEnMano + new Vector3(50f, 30f, 0f));
    }
}
