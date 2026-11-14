using System;
using UnityEngine;

public class Inventario : MonoBehaviour
{
    [SerializeField] private int cantidadSlots=4;
    [SerializeField] private int cargadoresSlot=4;
    [SerializeField] private int cargadores=0;
    [SerializeField] private Arma[] armas = new Arma[4];



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public int slotsUsados()
    {
        int usados=0;

        for(int i =0; i<armas.Length; i++)
        {
            if (armas[i] != null)
            {
                usados += armas[i].slotsInventario;
            }

        }
        usados+=(cargadores+cargadoresSlot -1) / cargadoresSlot;
        return usados;
    }
    public int slotsLibres()
    {
    return cantidadSlots - slotsUsados();
    }

    


}
