using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Se agrega a cada botón: suena al pasar el mouse por encima y al hacer clic.
[RequireComponent(typeof(Button))]
public class SonidoBoton : MonoBehaviour, IPointerEnterHandler
{
    GestorAudioMenu audioMenu;

    void Start()
    {
        audioMenu = FindAnyObjectByType<GestorAudioMenu>();
        GetComponent<Button>().onClick.AddListener(AlHacerClick);
    }

    public void OnPointerEnter(PointerEventData datos)
    {
        if (audioMenu != null) audioMenu.ReproducirHover();
    }

    void AlHacerClick()
    {
        if (audioMenu != null) audioMenu.ReproducirClick();
    }
}
