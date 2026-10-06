using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controla el menú principal: botones, paneles (opciones y créditos) y el paso al juego.
public class MenuPrincipal : MonoBehaviour
{
    [Header("Escena del juego")]
    public string escenaJuego = "Sotano_Jugable";

    [Header("Paneles")]
    public GameObject panelPrincipal;
    public GameObject panelOpciones;
    public GameObject panelCreditos;

    [Header("Botones")]
    public Button botonJugar;
    public Button botonOpciones;
    public Button botonCreditos;
    public Button botonSalir;
    public Button botonVolverOpciones;
    public Button botonVolverCreditos;

    [Header("Fundido a negro")]
    public CanvasGroup fundido;
    public float duracionFundido = 1f;

    bool cargando;

    void Start()
    {
        // En el menú el cursor tiene que verse (en el juego se ocultará).
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        botonJugar.onClick.AddListener(Jugar);
        botonOpciones.onClick.AddListener(AbrirOpciones);
        botonCreditos.onClick.AddListener(AbrirCreditos);
        botonSalir.onClick.AddListener(Salir);
        botonVolverOpciones.onClick.AddListener(VolverAlMenu);
        botonVolverCreditos.onClick.AddListener(VolverAlMenu);

        VolverAlMenu();
        StartCoroutine(Fundir(1f, 0f)); // al abrir, la pantalla pasa de negro a normal
    }

    void Update()
    {
        // Con Escape se vuelve desde opciones o créditos.
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame
            && !panelPrincipal.activeSelf)
        {
            VolverAlMenu();
        }
    }

    void Jugar()
    {
        if (!cargando) StartCoroutine(CargarJuego());
    }

    void AbrirOpciones()
    {
        MostrarSoloPanel(panelOpciones);
    }

    void AbrirCreditos()
    {
        MostrarSoloPanel(panelCreditos);
    }

    void VolverAlMenu()
    {
        MostrarSoloPanel(panelPrincipal);
    }

    void MostrarSoloPanel(GameObject panel)
    {
        panelPrincipal.SetActive(panel == panelPrincipal);
        panelOpciones.SetActive(panel == panelOpciones);
        panelCreditos.SetActive(panel == panelCreditos);
    }

    void Salir()
    {
        Debug.Log("Saliendo del juego...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // dentro del editor solo se detiene el Play
#else
        Application.Quit();
#endif
    }

    // Oscurece la pantalla y después carga la escena del juego.
    IEnumerator CargarJuego()
    {
        cargando = true;
        yield return Fundir(0f, 1f);
        SceneManager.LoadScene(escenaJuego);
    }

    // Cambia la opacidad del fundido de un valor a otro en "duracionFundido" segundos.
    IEnumerator Fundir(float desde, float hasta)
    {
        float tiempo = 0f;
        fundido.blocksRaycasts = true;
        while (tiempo < duracionFundido)
        {
            tiempo += Time.deltaTime;
            fundido.alpha = Mathf.Lerp(desde, hasta, tiempo / duracionFundido);
            yield return null;
        }
        fundido.alpha = hasta;
        fundido.blocksRaycasts = hasta > 0.5f; // si ya está transparente, no bloquea los clics
    }
}
