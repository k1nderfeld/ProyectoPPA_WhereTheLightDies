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

    // Crea una caja de munición en el suelo (por ejemplo, al botarla del inventario)
    public static void CrearCaja(TipoMunicion tipo, int cantidad, Vector3 posicion)
    {
        GameObject caja = GameObject.CreatePrimitive(PrimitiveType.Cube);
        caja.name = tipo == TipoMunicion.Cargador ? "Cargadores" : "Cartuchos";
        caja.transform.position = posicion + Vector3.up * 0.06f;
        caja.transform.localScale = new Vector3(0.22f, 0.12f, 0.15f);
        caja.GetComponent<BoxCollider>().isTrigger = true; // no choca con el jugador

        // Verde para cargadores y rojo para cartuchos, igual que las cajas del nivel
        Color color = tipo == TipoMunicion.Cargador ? new Color(0.25f, 0.32f, 0.18f) : new Color(0.45f, 0.08f, 0.06f);
        caja.GetComponent<Renderer>().material.color = color;

        Municion municion = caja.AddComponent<Municion>();
        municion.tipo = tipo;
        municion.cantidad = cantidad;
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
