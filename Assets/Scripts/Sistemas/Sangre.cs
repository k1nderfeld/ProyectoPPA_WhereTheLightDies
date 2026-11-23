using UnityEngine;

// Efectos de sangre (sin HUD, todo se ve en el mundo):
// - Impacto: salpican unas gotas desde donde pegó la bala.
// - Muerte: un estallido grande y una mancha en el suelo.
// Las gotas son cubitos rojos con física que caen y desaparecen solos.
// Es "static": se llama desde cualquier script con Sangre.Impacto(...) sin poner nada en la escena.
public static class Sangre
{
    static Material materialSangre;     // un solo material compartido (más liviano)
    static int gotasVivas;              // límite para no llenar la escena de objetos
    const int MaximoGotas = 120;

    // Salpicadura pequeña al acertar un disparo
    public static void Impacto(Vector3 punto, Vector3 normal)
    {
        Salpicar(punto, normal, 5, 3f);
    }

    // Estallido grande al morir un enemigo, más una mancha en el suelo
    public static void Muerte(Vector3 posicion)
    {
        Salpicar(posicion + Vector3.up * 1f, Vector3.up, 18, 5f);
        Mancha(posicion);
    }

    static void Salpicar(Vector3 punto, Vector3 normal, int cantidad, float fuerza)
    {
        for (int i = 0; i < cantidad; i++)
        {
            if (gotasVivas >= MaximoGotas) return;

            GameObject gota = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gota.name = "Gota";
            gota.transform.position = punto + normal * 0.05f;
            gota.transform.localScale = Vector3.one * Random.Range(0.03f, 0.08f);
            gota.GetComponent<Renderer>().sharedMaterial = ObtenerMaterial(gota);

            // Sale disparada hacia afuera de la herida, con algo de azar
            Rigidbody rb = gota.AddComponent<Rigidbody>();
            rb.mass = 0.05f;
            Vector3 direccion = (normal + Random.insideUnitSphere * 0.8f).normalized;
            rb.linearVelocity = direccion * Random.Range(fuerza * 0.4f, fuerza);

            // Capa "Ignore Raycast": las balas y la visión de los enemigos no chocan con las gotas
            gota.layer = 2;
            gota.AddComponent<GotaSangre>();
            gotasVivas++;
        }
    }

    // Charco oscuro en el suelo (un Quad acostado). Dura 20 segundos.
    static void Mancha(Vector3 posicion)
    {
        RaycastHit suelo;
        if (!Physics.Raycast(posicion + Vector3.up * 0.5f, Vector3.down, out suelo, 3f, ~4, QueryTriggerInteraction.Ignore))
            return;

        GameObject charco = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Object.Destroy(charco.GetComponent<Collider>());
        charco.name = "Charco";
        charco.transform.position = suelo.point + suelo.normal * 0.01f;
        charco.transform.rotation = Quaternion.LookRotation(-suelo.normal) * Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        charco.transform.localScale = Vector3.one * Random.Range(1f, 1.6f);
        charco.GetComponent<Renderer>().sharedMaterial = ObtenerMaterial(charco);
        Object.Destroy(charco, 20f);
    }

    // Crea el material rojo oscuro la primera vez, copiando el material normal de Unity
    static Material ObtenerMaterial(GameObject ejemplo)
    {
        if (materialSangre == null)
        {
            materialSangre = new Material(ejemplo.GetComponent<Renderer>().sharedMaterial);
            materialSangre.color = new Color(0.35f, 0f, 0f);
        }
        return materialSangre;
    }

    // Lo llama cada gota al desaparecer
    public static void GotaDestruida()
    {
        gotasVivas--;
    }
}
