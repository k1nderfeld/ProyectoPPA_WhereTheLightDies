using UnityEngine;

// Pistola: un disparo preciso por clic. Usa cargadores de 6 balas y ocupa 1 slot (RF-13).
// Clase "Pistola" del diagrama de clases. Se agrega al objeto de la pistola.
public class Pistola : Arma
{
    public override void ValoresIniciales()
    {
        danioPorRayo = 20f;
        rango = 40f;
        cantidadRayosAtaque = 1;
        cadencia = 3f;
        tiempoRecarga = 1.5f;
        municionPorCarga = 6;
        municionActual = 6;
        slotsInventario = 1;
        tipoMunicion = TipoMunicion.Cargador;
        posicionEnMano = new Vector3(0.2f, -0.2f, 0.45f);
        rotacionEnMano = new Vector3(0f, -4f, 0f);
    }
}
