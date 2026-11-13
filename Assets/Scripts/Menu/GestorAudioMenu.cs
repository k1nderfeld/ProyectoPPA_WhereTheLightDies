using UnityEngine;

// Reproduce la música de ambiente y los sonidos de los botones del menú.
// Necesita dos AudioSource: uno para la música (en bucle) y otro para los efectos.
public class GestorAudioMenu : MonoBehaviour
{
    public AudioSource musica;
    public AudioSource efectos;
    public AudioClip sonidoHover;
    public AudioClip sonidoClick;

    void Start()
    {
        Ajustes.Aplicar();
        ActualizarVolumenes();
        musica.loop = true;
        musica.Play();
    }

    // Se llama al iniciar y cada vez que el jugador mueve un slider de volumen.
    public void ActualizarVolumenes()
    {
        AudioListener.volume = Ajustes.VolumenGeneral;
        musica.volume = Ajustes.VolumenMusica;
        efectos.volume = Ajustes.VolumenEfectos;
    }

    public void ReproducirHover()
    {
        efectos.PlayOneShot(sonidoHover);
    }

    public void ReproducirClick()
    {
        efectos.PlayOneShot(sonidoClick);
    }
}
