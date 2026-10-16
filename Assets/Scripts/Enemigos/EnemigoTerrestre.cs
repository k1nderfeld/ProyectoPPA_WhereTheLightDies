using UnityEngine;
using UnityEngine.AI;

public class EnemigoTerrestre : MonoBehaviour
{
    [SerializeField] private float conoVision =90f; //En grados
    [SerializeField] private float velocidadPatrulla = 2f;
    [SerializeField] private float velocidadPersecucion = 4f;
    [SerializeField] private float rangoAtaque = 1.5f; //
    [SerializeField] private float danio= 10f;
    [SerializeField] private float tiempoEntreAtaques= 1f;
    [SerializeField] private int cantidadRayos = 10;
    private NavMeshAgent patrullaje;
    Transform jugador;
    Salud saludJugador;
    [SerializeField] private float salud = 50f;
    [SerializeField] private Transform cabeza;

    //todos los puntos donde puede caminar el enemigo usando NavMesh
    Vector3[] puntosPatrullaje;
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // se obtiene el agente que contra al enemigo
        patrullaje = GetComponent<NavMeshAgent>(); 
        // busca un jugador si tiene el atrubuto salud;  
        saludJugador=FindFirstObjectByType<Salud>();
        // guarda la posibilidad de cambiar la salud del jugador solo si existe
        if(saludJugador!=null){
            jugador=saludJugador.transform;

        }
        //Calcula todos los puntos patrullables
        puntosPatrullaje = NavMesh.CalculateTriangulation().vertices;
    }

    // Update is called once per frame
    void Update()
    {
        //si no hay jugador no hace nada
        if(jugador==null) {
            return;
        }
        //si algun raycast de veAlJuagor llega al jugador persigue
        if(veAlJugador()==true){
            perseguir();
        }else{
            patrullar();
        }

        
    }
    
    public void RecibirDanio(float danioRecibido)
    {
        salud -= danioRecibido;
        if (salud <= 0f) {
            Morir();
            }
    }

    void SoltarMunicion(){

    }
    void Morir(){
    // SoltarMunicion() pendiente
    Destroy(gameObject);
    }
    void patrullar(){
        patrullaje.speed = velocidadPatrulla;
    }
    
    //Metodo para detectar si el jugador esta en el cono de vision enemigo
    //Revisar
    bool veAlJugador()
    {
        if(jugador==null) return false;
        for (int i =0; i<10; i++)
        {
            // en cada iteracion se calculan rayos desde -45 a 45 grados
         float anguloDeRayo = -45f + (i*10f);


         // gira desde donde esta la cabeza a anguloDeRayo
         Vector3 direccionDeRayoEnI= Quaternion.Euler(0f,anguloDeRayo,0f) * cabeza.forward;


         RaycastHit hit;
         if(Physics.Raycast(cabeza.position, direccionDeRayoEnI, out hit ) && hit.transform==jugador)
        {
            return true;
            }
        }
        return false;
    }
    bool perseguir()
    {
        return false;
    }


}
