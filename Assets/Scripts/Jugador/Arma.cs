using System;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Arma : MonoBehaviour
{

    public float danioPorRayo;
    public float rango;

    // Pistola 1 rayo
    // Escopeta multiples rayos 1 por cada perdigon
    // Cuerpo a cuerpo multiples en un swing
    public int cantidadRayosAtaque;
    public float cadencia;
    public float tiempoRecarga;
    public int municionPorCarga;
    //Slots que usara dentro del inventario
    public int slotsInventario;

    public int municionActual;
    protected float proximoUso;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public abstract void usar();


    // para saber cuando ataca otra vez
    public bool sePuedeUsar()
    {
        return Time.time>=proximoUso;
        
    }



}
