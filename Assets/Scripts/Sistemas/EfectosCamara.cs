using System.Collections;
using UnityEngine;

// Efectos que hacen que la acción "pese":
// - Sacudir: la cámara tiembla (al recibir un golpe o al disparar).
// - Congelar: el juego se detiene una fracción de segundo al matar (hitstop).
// No hay que ponerlo en la escena: se agrega solo a la cámara principal la primera vez que se usa.
public class EfectosCamara : MonoBehaviour
{
    static EfectosCamara instancia;

    Vector3 posicionOriginal;
    float fuerzaSacudida;
    bool congelado;

    static EfectosCamara Obtener()
    {
        if (instancia == null && Camera.main != null)
            instancia = Camera.main.gameObject.AddComponent<EfectosCamara>();
        return instancia;
    }

    // intensidad: 0.02 = suave (disparo), 0.15 = fuerte (golpe)
    public static void Sacudir(float intensidad)
    {
        EfectosCamara efectos = Obtener();
        if (efectos != null)
            efectos.fuerzaSacudida = Mathf.Max(efectos.fuerzaSacudida, intensidad);
    }

    // Detiene el tiempo un instante (por ejemplo 0.06 segundos)
    public static void Congelar(float segundos)
    {
        EfectosCamara efectos = Obtener();
        if (efectos != null && !efectos.congelado && Time.timeScale > 0f)
            efectos.StartCoroutine(efectos.Pausita(segundos));
    }

    void Awake()
    {
        posicionOriginal = transform.localPosition;
    }

    void LateUpdate()
    {
        if (fuerzaSacudida > 0.001f)
        {
            // Mueve la cámara a un punto al azar cerca de su lugar, y la fuerza baja rápido
            transform.localPosition = posicionOriginal + Random.insideUnitSphere * fuerzaSacudida;
            fuerzaSacudida = Mathf.Lerp(fuerzaSacudida, 0f, 12f * Time.unscaledDeltaTime);
        }
        else
        {
            fuerzaSacudida = 0f;
            transform.localPosition = posicionOriginal;
        }
    }

    IEnumerator Pausita(float segundos)
    {
        congelado = true;
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(segundos);
        if (Time.timeScale > 0f) Time.timeScale = 1f; // si en ese instante se abrió la pausa, no la quita
        congelado = false;
    }
}
