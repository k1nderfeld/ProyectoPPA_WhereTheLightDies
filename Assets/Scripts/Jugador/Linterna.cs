using UnityEngine;
using UnityEngine.InputSystem;

// Linterna pegada al arma/cámara. Se prende y apaga con F.
// Ilumina hacia donde mira el jugador, sin sombras (así no pesa en PC de gama baja).
// Se agrega al objeto "Jugador". Si no se asigna una luz, la crea sola en la cámara.
public class Linterna : MonoBehaviour
{
    public Light luz;                       // luz tipo Spot (puede quedar vacía)
    public float intensidad = 6f;
    public float alcance = 18f;
    public float angulo = 55f;
    public AudioClip sonidoClic;            // opcional

    void Start()
    {
        if (luz == null) luz = CrearLuz();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;   // en pausa no se usa
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            luz.enabled = !luz.enabled;
            if (sonidoClic != null)
                AudioSource.PlayClipAtPoint(sonidoClic, transform.position, 0.5f * Ajustes.VolumenEfectos);
        }
    }

    // Crea la luz como hija de la cámara, un poco abajo a la derecha (donde va el arma)
    Light CrearLuz()
    {
        GameObject objeto = new GameObject("Linterna_Luz");
        objeto.transform.SetParent(Camera.main.transform, false);
        objeto.transform.localPosition = new Vector3(0.25f, -0.2f, 0.3f);

        Light nueva = objeto.AddComponent<Light>();
        nueva.type = LightType.Spot;
        nueva.intensity = intensidad;
        nueva.range = alcance;
        nueva.spotAngle = angulo;
        nueva.innerSpotAngle = angulo * 0.5f;
        nueva.color = new Color(1f, 0.95f, 0.85f); // blanco cálido, de pilas
        nueva.shadows = LightShadows.None;
        return nueva;
    }
}
