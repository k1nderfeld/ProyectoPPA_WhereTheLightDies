using UnityEngine;
using UnityEngine.UI;

// Hace que un texto parpadee como una luz que está fallando (para el título).
[RequireComponent(typeof(Text))]
public class TextoParpadeo : MonoBehaviour
{
    public float velocidad = 6f;        // qué tan rápido cambia
    public float brilloMinimo = 0.45f;  // 0 = se apaga del todo, 1 = no parpadea

    Text texto;
    Color colorOriginal;

    void Awake()
    {
        texto = GetComponent<Text>();
        colorOriginal = texto.color;
    }

    void Update()
    {
        // El ruido de Perlin da valores entre 0 y 1 que cambian de forma irregular.
        float ruido = Mathf.PerlinNoise(Time.time * velocidad, 0f);
        float brillo = Mathf.Lerp(brilloMinimo, 1f, ruido);
        texto.color = new Color(colorOriginal.r, colorOriginal.g, colorOriginal.b, colorOriginal.a * brillo);
    }
}
