using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Muestra el daño sin HUD (RF-08): una viñeta roja alrededor de la pantalla.
// - Con la salud llena no se ve.
// - Mientras menos salud, más fuerte queda.
// - Cada golpe hace un destello rojo que se desvanece en medio segundo.
// Se agrega al objeto "PostProceso" (el que tiene el Volume).
[RequireComponent(typeof(Volume))]
public class EfectoDanio : MonoBehaviour
{
    public float vinetaMaxima = 0.5f;    // con la salud casi en 0
    public float destelloGolpe = 0.3f;   // lo que sube justo al recibir un golpe
    public float duracionDestello = 0.5f;

    Protagonista protagonista;
    Vignette vineta;

    void Start()
    {
        protagonista = FindAnyObjectByType<Protagonista>();

        // "profile" crea una copia del perfil solo para esta partida,
        // así no se cambia el archivo PerfilSotano guardado en el proyecto
        Volume volumen = GetComponent<Volume>();
        volumen.profile.TryGet(out vineta);
        if (vineta != null)
        {
            vineta.color.Override(new Color(0.6f, 0f, 0f));
            vineta.intensity.Override(0f);
        }
    }

    void Update()
    {
        if (vineta == null || protagonista == null) return;

        // Parte que depende de la salud que falta (0 = sano, 1 = casi muerto)
        float saludPerdida = 1f - protagonista.SaludActual / protagonista.SaludMaxima;
        float intensidad = saludPerdida * vinetaMaxima;

        // Destello del último golpe, que se apaga con el tiempo
        float tiempoDesdeGolpe = Time.time - protagonista.TiempoUltimoDanio;
        if (tiempoDesdeGolpe < duracionDestello)
            intensidad += destelloGolpe * (1f - tiempoDesdeGolpe / duracionDestello);

        vineta.intensity.Override(Mathf.Clamp01(intensidad));
    }
}
