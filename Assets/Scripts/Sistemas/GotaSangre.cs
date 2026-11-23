using UnityEngine;

// Cada gota de sangre se borra sola después de un rato (la crea Sangre.cs)
public class GotaSangre : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, Random.Range(1.5f, 3f));
    }

    void OnDestroy()
    {
        Sangre.GotaDestruida();
    }
}
