using UnityEngine;

// Hace que una luz parpadee como una ampolleta vieja: la mayor parte del tiempo está encendida
// y de vez en cuando baja de golpe por un instante. Se agrega a un objeto con Light.
[RequireComponent(typeof(Light))]
public class LuzParpadeo : MonoBehaviour
{
    public float probabilidadApagon = 0.04f; // probabilidad por cuadro de que parpadee
    public float intensidadApagada = 0.2f;    // cuánto baja (0 = se apaga del todo)

    Light luz;
    float intensidadNormal;
    float finApagon;

    void Start()
    {
        luz = GetComponent<Light>();
        intensidadNormal = luz.intensity;
    }

    void Update()
    {
        if (Time.time < finApagon)
        {
            luz.intensity = intensidadNormal * intensidadApagada;
            return;
        }

        luz.intensity = intensidadNormal;
        if (Random.value < probabilidadApagon)
            finApagon = Time.time + Random.Range(0.05f, 0.25f); // apagón corto
    }
}
