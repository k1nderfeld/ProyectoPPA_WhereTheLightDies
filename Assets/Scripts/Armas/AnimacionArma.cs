using UnityEngine;
using UnityEngine.InputSystem;

// Hace que el arma en la mano se vea viva, moviendo el objeto "Mano" (donde están las armas):
// - se balancea al caminar (más al correr),
// - se atrasa un poco al girar la vista con el mouse (inercia),
// - sube desde abajo al equiparla y baja e inclina al recargar.
// El retroceso al disparar lo hace cada arma (ver Arma.Retroceso).
// Se agrega al objeto "Mano" (hijo de la cámara).
public class AnimacionArma : MonoBehaviour
{
    public Protagonista protagonista;

    [Header("Balanceo al caminar")]
    public float ritmoPasos = 8f;           // qué tan rápido se balancea
    public float alturaBalanceo = 0.012f;   // cuánto sube y baja, en metros
    public float multiplicadorCorrer = 1.6f;

    [Header("Inercia al mirar")]
    public float inercia = 0.04f;           // grados por cada píxel que se mueve el mouse
    public float inerciaMaxima = 4f;        // tope, en grados

    [Header("Suavidad")]
    public float suavidad = 10f;            // más alto = vuelve más rápido a su lugar

    float tiempoPasos;      // avanza mientras se camina; marca el ritmo del balanceo
    float bajada;           // cuánto está bajada el arma (al equipar o recargar)
    Arma armaAnterior;

    void Update()
    {
        if (Time.timeScale == 0f) return; // en pausa o con el inventario abierto no se mueve

        Arma arma = protagonista.ArmaEquipada;

        // Al cambiar de arma, la nueva sube desde abajo
        if (arma != armaAnterior)
        {
            bajada = 0.35f;
            armaAnterior = arma;
        }

        // 1) Balanceo: movimiento en forma de "8" mientras se camina
        Vector3 balanceo = Vector3.zero;
        if (protagonista.SeMueve)
        {
            float rapidez = protagonista.EstaCorriendo ? multiplicadorCorrer : 1f;
            tiempoPasos += Time.deltaTime * ritmoPasos * rapidez;
            balanceo.x = Mathf.Cos(tiempoPasos * 0.5f) * alturaBalanceo * rapidez;
            balanceo.y = Mathf.Sin(tiempoPasos) * alturaBalanceo * rapidez;
        }

        // 2) Recarga: el arma baja y se inclina; si no, vuelve a su lugar
        bool recargando = arma != null && arma.Recargando;
        float bajadaObjetivo = recargando ? 0.12f : 0f;
        bajada = Mathf.Lerp(bajada, bajadaObjetivo, suavidad * Time.deltaTime);
        float inclinacion = recargando ? 25f : 0f;

        // 3) Inercia: el arma se atrasa un poco cuando se gira la vista
        Vector2 movimientoMouse = Vector2.zero;
        if (Mouse.current != null) movimientoMouse = Mouse.current.delta.ReadValue();
        float giroVertical = Mathf.Clamp(movimientoMouse.y * inercia, -inerciaMaxima, inerciaMaxima);
        float giroHorizontal = Mathf.Clamp(-movimientoMouse.x * inercia, -inerciaMaxima, inerciaMaxima);

        // Se mueve suave hacia la posición y el giro que corresponden
        Vector3 posicionObjetivo = balanceo + new Vector3(0f, -bajada, 0f);
        Quaternion giroObjetivo = Quaternion.Euler(giroVertical, giroHorizontal, inclinacion);
        transform.localPosition = Vector3.Lerp(transform.localPosition, posicionObjetivo, suavidad * Time.deltaTime);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, giroObjetivo, suavidad * Time.deltaTime);
    }
}
