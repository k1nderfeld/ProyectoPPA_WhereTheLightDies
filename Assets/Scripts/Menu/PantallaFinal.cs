using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Muestra la pantalla de derrota cuando el protagonista muere
// y la de victoria cuando llega a la salida (la llama SalidaNivel).
// Desde las dos se puede reintentar el nivel o volver al menú principal.
// Se agrega al objeto "Canvas_Final" de la escena de juego.
public class PantallaFinal : MonoBehaviour
{
    public GameObject panelDerrota;
    public GameObject panelVictoria;
    public Button botonReintentarDerrota;
    public Button botonMenuDerrota;
    public Button botonReintentarVictoria;
    public Button botonMenuVictoria;
    public string escenaMenu = "MenuPrincipal";
    public float esperaDerrota = 1.5f; // segundos entre la muerte y la pantalla

    // La pausa la revisa para no abrirse encima de esta pantalla
    public static bool Terminado { get; private set; }

    Protagonista protagonista;
    float tiempoMuerte = -1f;

    void Start()
    {
        Terminado = false;
        protagonista = FindAnyObjectByType<Protagonista>();

        panelDerrota.SetActive(false);
        panelVictoria.SetActive(false);

        botonReintentarDerrota.onClick.AddListener(Reintentar);
        botonReintentarVictoria.onClick.AddListener(Reintentar);
        botonMenuDerrota.onClick.AddListener(VolverAlMenu);
        botonMenuVictoria.onClick.AddListener(VolverAlMenu);
    }

    void Update()
    {
        if (Terminado || protagonista == null) return;

        // Cuando muere, espera un poco y muestra la derrota
        if (protagonista.EstaMuerto)
        {
            if (tiempoMuerte < 0f) tiempoMuerte = Time.time;
            if (Time.time - tiempoMuerte >= esperaDerrota) Mostrar(panelDerrota);
        }
    }

    // La llama SalidaNivel cuando el jugador escapa del sótano
    public void Ganar()
    {
        if (Terminado || protagonista.EstaMuerto) return;
        Mostrar(panelVictoria);
    }

    void Mostrar(GameObject panel)
    {
        Terminado = true;
        panel.SetActive(true);
        Time.timeScale = 0f;                    // detiene el juego
        Cursor.lockState = CursorLockMode.None; // muestra el cursor para los botones
        Cursor.visible = true;
    }

    void Reintentar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // carga el mismo nivel de nuevo
    }

    void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(escenaMenu);
    }
}
