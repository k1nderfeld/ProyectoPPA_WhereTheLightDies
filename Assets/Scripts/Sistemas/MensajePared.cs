using UnityEngine;

// Texto escrito en la pared (tutorial sin HUD).
// Está invisible y aparece de a poco, con un parpadeo, cuando el jugador se acerca.
// Se agrega a un objeto con TextMesh (texto 3D).
[RequireComponent(typeof(TextMesh))]
public class MensajePared : MonoBehaviour
{
    public float distanciaAparicion = 7f;   // a esta distancia empieza a verse
    public float velocidadAparicion = 1.5f; // qué tan rápido aparece

    TextMesh texto;
    Transform jugador;
    Color colorBase;
    float visible;                          // 0 = invisible, 1 = se ve completo

    void Start()
    {
        texto = GetComponent<TextMesh>();
        colorBase = texto.color;
        Protagonista protagonista = FindAnyObjectByType<Protagonista>();
        if (protagonista != null) jugador = protagonista.transform;
        Pintar(0f);
    }

    void Update()
    {
        if (jugador == null) return;

        // Una vez que apareció, se queda (sube hasta 1 y no vuelve a bajar)
        if (Vector3.Distance(jugador.position, transform.position) <= distanciaAparicion)
            visible = Mathf.MoveTowards(visible, 1f, velocidadAparicion * Time.deltaTime);

        // Parpadeo: de vez en cuando el texto baja de golpe, como una luz que falla
        float parpadeo = Random.value < 0.03f ? 0.3f : 1f;
        Pintar(visible * parpadeo);
    }

    void Pintar(float alfa)
    {
        texto.color = new Color(colorBase.r, colorBase.g, colorBase.b, alfa);
    }
}
