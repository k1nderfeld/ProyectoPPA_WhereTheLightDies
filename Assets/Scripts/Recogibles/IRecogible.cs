// Algo que el protagonista puede recoger del suelo con la tecla E, a 2 m o menos (RF-10).
// Lo implementan las armas y la munición. Es una interfaz: solo dice QUÉ deben tener,
// y cada clase decide CÓMO lo hace.
public interface IRecogible
{
    // Texto del aviso en pantalla, por ejemplo "Recoger pistola"
    string Descripcion { get; }

    // Intenta recogerlo. Devuelve true si se recogió (false si no había espacio).
    bool Recoger(Protagonista protagonista);
}
