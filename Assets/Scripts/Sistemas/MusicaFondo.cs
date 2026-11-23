using UnityEngine;

// Aplica el volumen de música guardado en Ajustes a la música de fondo.
// El volumen general ya lo aplica AudioListener.volume (vía Ajustes.Aplicar()).
[RequireComponent(typeof(AudioSource))]
public class MusicaFondo : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float volumenBase = 0.4f; // volumen propio de esta pista

    AudioSource fuente;

    // Lo activa el GestorOleadas: durante una oleada la música sube y se acelera un poco
    public static bool EnCombate;
    float intensidad;               // va de 0 (calma) a 1 (combate), cambia de a poco

    void Awake()
    {
        fuente = GetComponent<AudioSource>();
    }

    void Update()
    {
        intensidad = Mathf.MoveTowards(intensidad, EnCombate ? 1f : 0f, 0.5f * Time.deltaTime);
        fuente.volume = volumenBase * Ajustes.VolumenMusica * (1f + 0.6f * intensidad);
        fuente.pitch = 1f + 0.08f * intensidad;
    }
}