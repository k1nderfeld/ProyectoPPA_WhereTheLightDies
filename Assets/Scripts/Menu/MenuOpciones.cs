using UnityEngine;
using UnityEngine.UI;

// Controla la pantalla de opciones: sliders de volumen, sensibilidad y pantalla completa.
// Cada cambio se guarda en Ajustes y se aplica de inmediato.
public class MenuOpciones : MonoBehaviour
{
    public Slider sliderGeneral;
    public Slider sliderMusica;
    public Slider sliderEfectos;
    public Slider sliderSensibilidad;
    public Toggle togglePantallaCompleta;

    // Textos que muestran el valor junto a cada slider (por ejemplo "80%")
    public Text textoGeneral;
    public Text textoMusica;
    public Text textoEfectos;
    public Text textoSensibilidad;

    public GestorAudioMenu audioMenu;

    void Start()
    {
        // Primero se cargan los valores guardados en los controles...
        sliderGeneral.value = Ajustes.VolumenGeneral;
        sliderMusica.value = Ajustes.VolumenMusica;
        sliderEfectos.value = Ajustes.VolumenEfectos;
        sliderSensibilidad.value = Ajustes.Sensibilidad;
        togglePantallaCompleta.isOn = Ajustes.PantallaCompleta;
        ActualizarTextos();

        // ...y recién después se escuchan los cambios del jugador.
        sliderGeneral.onValueChanged.AddListener(CambioGeneral);
        sliderMusica.onValueChanged.AddListener(CambioMusica);
        sliderEfectos.onValueChanged.AddListener(CambioEfectos);
        sliderSensibilidad.onValueChanged.AddListener(CambioSensibilidad);
        togglePantallaCompleta.onValueChanged.AddListener(CambioPantallaCompleta);
    }

    void CambioGeneral(float valor)
    {
        Ajustes.VolumenGeneral = valor;
        ActualizarTodo();
    }

    void CambioMusica(float valor)
    {
        Ajustes.VolumenMusica = valor;
        ActualizarTodo();
    }

    void CambioEfectos(float valor)
    {
        Ajustes.VolumenEfectos = valor;
        ActualizarTodo();
    }

    void CambioSensibilidad(float valor)
    {
        Ajustes.Sensibilidad = valor;
        ActualizarTextos();
    }

    void CambioPantallaCompleta(bool activada)
    {
        Ajustes.PantallaCompleta = activada;
        Ajustes.Aplicar();
    }

    void ActualizarTodo()
    {
        audioMenu.ActualizarVolumenes();
        ActualizarTextos();
    }

    void ActualizarTextos()
    {
        textoGeneral.text = Porcentaje(sliderGeneral.value);
        textoMusica.text = Porcentaje(sliderMusica.value);
        textoEfectos.text = Porcentaje(sliderEfectos.value);
        textoSensibilidad.text = sliderSensibilidad.value.ToString("0.0");
    }

    string Porcentaje(float valor)
    {
        return Mathf.RoundToInt(valor * 100f) + "%";
    }

    // Se guarda al cerrar el panel de opciones.
    void OnDisable()
    {
        Ajustes.Guardar();
    }
}
