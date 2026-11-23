using UnityEngine;

// Botiquín que sueltan los enemigos. Se recoge solo al pasar por encima (sin apretar E),
// así el jugador no tiene que detenerse en medio de la pelea.
// Si el jugador ya tiene la salud llena, el botiquín se queda en el suelo.
public class Curacion : MonoBehaviour
{
    public float cantidad = 25f;   // cuánta salud recupera
    float giro;

    // Crea un botiquín en el suelo (lo usa Enemigo al morir)
    public static void Crear(Vector3 posicion)
    {
        GameObject botiquin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        botiquin.name = "Botiquin";
        botiquin.transform.position = posicion + Vector3.up * 0.25f;
        botiquin.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        botiquin.GetComponent<BoxCollider>().isTrigger = true;
        botiquin.GetComponent<Renderer>().material.color = new Color(0.9f, 0.9f, 0.9f);
        botiquin.AddComponent<Curacion>();
        Destroy(botiquin, 30f); // si nadie lo recoge, desaparece
    }

    void Update()
    {
        // Gira despacio para que se note que se puede recoger
        transform.Rotate(0f, 90f * Time.deltaTime, 0f);
    }

    void OnTriggerEnter(Collider otro)
    {
        Protagonista protagonista = otro.GetComponent<Protagonista>();
        if (protagonista == null) return;

        if (protagonista.Curar(cantidad))
        {
            protagonista.MostrarMensaje("+" + cantidad + " de salud");
            Destroy(gameObject);
        }
    }

    // Si el jugador ya estaba encima con la salud llena y después recibe daño, lo recoge igual
    void OnTriggerStay(Collider otro)
    {
        OnTriggerEnter(otro);
    }
}
