using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Pausa el juego con Escape. Desde la pausa se puede reanudar, abrir las opciones
// (volumen, sensibilidad, pantalla completa) o volver al menú principal.
// Se agrega al objeto "Canvas_Pausa" de la escena de juego.
public class MenuPausa : MonoBehaviour
{
    public GameObject panelPausa;
    public Button botonReanudar;
    public Button botonOpciones;
    public Button botonMenu;
    public string escenaMenu = "MenuPrincipal";

    [Header("Opciones (el mismo panel que en el menú principal)")]
    public GameObject panelOpciones;
    public Button botonVolverOpciones;

    bool enPausa;

    void Start()
    {
        botonReanudar.onClick.AddListener(Reanudar);
        botonMenu.onClick.AddListener(VolverAlMenu);
        if (botonOpciones != null) botonOpciones.onClick.AddListener(AbrirOpciones);
        if (botonVolverOpciones != null) botonVolverOpciones.onClick.AddListener(CerrarOpciones);
        Reanudar(); // el juego empieza sin pausa
    }

    void Update()
    {
        if (MenuInventario.EstaAbierto) return; // con el inventario abierto no se pausa
        if (PantallaFinal.Terminado) return;    // tampoco en la pantalla de derrota o victoria

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Escape en las opciones vuelve a la pausa; si no, pausa o reanuda
            if (panelOpciones != null && panelOpciones.activeSelf) CerrarOpciones();
            else if (enPausa) Reanudar();
            else Pausar();
        }
    }

    void Pausar()
    {
        enPausa = true;
        panelPausa.SetActive(true);
        Time.timeScale = 0f;                       // detiene el juego
        Cursor.lockState = CursorLockMode.None;    // muestra el cursor
        Cursor.visible = true;
    }

    void Reanudar()
    {
        enPausa = false;
        panelPausa.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(false);
        Time.timeScale = 1f;                       // el juego sigue
        Cursor.lockState = CursorLockMode.Locked;  // oculta el cursor
        Cursor.visible = false;
    }

    // Cambia el panel de pausa por el de opciones (el juego sigue detenido)
    void AbrirOpciones()
    {
        panelPausa.SetActive(false);
        panelOpciones.SetActive(true);
    }

    void CerrarOpciones()
    {
        panelOpciones.SetActive(false);
        panelPausa.SetActive(true);
    }

    void VolverAlMenu()
    {
        Time.timeScale = 1f; // si no, el menú quedaría congelado
        SceneManager.LoadScene(escenaMenu);
    }
}
