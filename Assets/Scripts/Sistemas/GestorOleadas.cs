using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controla las oleadas de enemigos del nivel.
// Cada oleada crea más enemigos que la anterior en los puntos de aparición.
// Cuando se terminan todas las oleadas, la salida del nivel se abre.
// Se pone en un objeto vacío de la escena (por ejemplo "GestorOleadas").
public class GestorOleadas : MonoBehaviour
{
    [Header("Enemigos")]
    public GameObject[] prefabsEnemigos;      // tipos de enemigo que pueden aparecer (se elige uno al azar)
    public Transform[] puntosAparicion;       // objetos vacíos donde nacen los enemigos

    [Header("Oleadas")]
    public int cantidadOleadas = 5;
    public int enemigosPrimeraOleada = 3;
    public int enemigosExtraPorOleada = 2;    // cada oleada trae esta cantidad más
    public int maximoVivosALaVez = 8;         // para no llenar la pantalla (y cuidar el rendimiento)
    public float tiempoEntreEnemigos = 0.8f;  // segundos entre un enemigo y el siguiente
    public float descansoEntreOleadas = 5f;   // respiro para recoger munición
    public float distanciaMinimaJugador = 6f; // no aparecen pegados al jugador

    public Protagonista protagonista;

    public bool Terminado { get; private set; }
    public int OleadaActual { get; private set; }

    List<Enemigo> vivos = new List<Enemigo>();

    void Start()
    {
        if (protagonista == null) protagonista = FindAnyObjectByType<Protagonista>();
        StartCoroutine(CicloDeOleadas());
    }

    IEnumerator CicloDeOleadas()
    {
        yield return new WaitForSeconds(3f); // un momento para ubicarse antes de la primera

        for (int oleada = 1; oleada <= cantidadOleadas; oleada++)
        {
            OleadaActual = oleada;
            protagonista.MostrarMensaje("Oleada " + oleada + " de " + cantidadOleadas);

            int total = enemigosPrimeraOleada + enemigosExtraPorOleada * (oleada - 1);
            int creados = 0;
            while (creados < total)
            {
                LimpiarMuertos();
                if (vivos.Count < maximoVivosALaVez)
                {
                    CrearEnemigo();
                    creados++;
                }
                yield return new WaitForSeconds(tiempoEntreEnemigos);
            }

            // Espera a que el jugador mate a todos los de esta oleada
            while (true)
            {
                LimpiarMuertos();
                if (vivos.Count == 0) break;
                yield return new WaitForSeconds(0.5f);
            }

            if (oleada < cantidadOleadas)
            {
                protagonista.MostrarMensaje("Oleada superada");
                yield return new WaitForSeconds(descansoEntreOleadas);
            }
        }

        Terminado = true;
        protagonista.MostrarMensaje("El sótano está despejado. Busca la salida");
    }

    void CrearEnemigo()
    {
        Transform punto = ElegirPunto();
        GameObject prefab = prefabsEnemigos[Random.Range(0, prefabsEnemigos.Length)];
        GameObject nuevo = Instantiate(prefab, punto.position, punto.rotation);

        Enemigo enemigo = nuevo.GetComponent<Enemigo>();
        if (enemigo != null)
        {
            enemigo.Alertar(); // sale directo a buscar al jugador
            vivos.Add(enemigo);
        }
    }

    // Elige un punto de aparición al azar que no esté demasiado cerca del jugador
    Transform ElegirPunto()
    {
        for (int intento = 0; intento < 10; intento++)
        {
            Transform punto = puntosAparicion[Random.Range(0, puntosAparicion.Length)];
            if (Vector3.Distance(punto.position, protagonista.transform.position) >= distanciaMinimaJugador)
                return punto;
        }
        return puntosAparicion[Random.Range(0, puntosAparicion.Length)];
    }

    // Saca de la lista a los enemigos que ya murieron
    void LimpiarMuertos()
    {
        for (int i = vivos.Count - 1; i >= 0; i--)
            if (vivos[i] == null || vivos[i].EstaMuerto)
                vivos.RemoveAt(i);
    }
}
