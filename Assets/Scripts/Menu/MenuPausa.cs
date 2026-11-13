using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Pausa el juego con Escape y permite volver al menú principal.
// Se agrega al objeto "Canvas_Pausa" de la escena de juego.
public class MenuPausa : MonoBehaviour
{
    public GameObject panelPausa;
    public Button botonReanudar;
    public Button botonMenu;
    public string escenaMenu = "MenuPrincipal";

    bool enPausa;

    void Start()
    {
        botonReanudar.onClick.AddListener(Reanudar);
        botonMenu.onClick.AddListener(VolverAlMenu);
        Reanudar(); // el juego empieza sin pausa
    }

    void Update()
    {
        if (MenuInventario.EstaAbierto) return; // con el inventario abierto no se pausa

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (enPausa) Reanudar();
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
        Time.timeScale = 1f;                       // el juego sigue
        Cursor.lockState = CursorLockMode.Locked;  // oculta el cursor
        Cursor.visible = false;
    }

    void VolverAlMenu()
    {
        Time.timeScale = 1f; // si no, el menú quedaría congelado
        SceneManager.LoadScene(escenaMenu);
    }
}
