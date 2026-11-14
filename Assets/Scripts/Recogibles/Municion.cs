using UnityEngine;

// Munición tirada en el nivel (una caja de cargadores o de cartuchos). Se recoge con E (RF-10).
// Usa Protagonista.RecogerMunicion, que guarda una unidad por vez en el inventario.
// Si no cabe todo, lo que sobra se queda en el suelo.
// Se agrega al objeto de la caja de munición.
public class Municion : MonoBehaviour, IRecogible
{
    public TipoMunicion tipo = TipoMunicion.Cargador;
    public int cantidad = 1;            // cuántos cargadores o cartuchos da

    public string Descripcion
    {
        get
        {
            string nombre = tipo == TipoMunicion.Cargador ? "cargadores" : "cartuchos";
            return "Recoger " + nombre + " (" + cantidad + ")";
        }
    }

    public bool Recoger(Protagonista protagonista)
    {
        while (cantidad > 0 && protagonista.RecogerMunicion(tipo))
            cantidad--;

        if (cantidad == 0)
        {
            Destroy(gameObject); // se recogió todo
            return true;
        }
        return false; // quedó munición en el suelo porque no había espacio
    }
}
