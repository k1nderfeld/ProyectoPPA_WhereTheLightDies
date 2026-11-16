using UnityEngine;

// Aplica el volumen de música guardado en Ajustes a la música de fondo.
// El volumen general ya lo aplica AudioListener.volume (vía Ajustes.Aplicar()).
[RequireComponent(typeof(AudioSource))]
public class MusicaFondo : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float volumenBase = 0.4f; // volumen propio de esta pista

    AudioSource fuente;

    void Awake()
    {
        fuente = GetComponent<AudioSource>();
    }

    void Update()
    {
        fuente.volume = volumenBase * Ajustes.VolumenMusica;
    }
}