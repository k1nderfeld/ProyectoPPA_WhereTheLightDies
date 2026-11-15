using UnityEngine;

// Un espacio del inventario. Guarda un arma, o munición de un solo tipo, o queda reservado
// (cuando un arma ocupa más de un espacio). Es la clase "Slot" del diagrama de clases.
// No se agrega a ningún objeto: el Inventario tiene sus propios slots.
[System.Serializable]
public class Slot
{
    // Capacidad fija de munición por slot (nota del diagrama de clases)
    public const int CapacidadCargadores = 4;
    public const int CapacidadCartuchos = 18;

    [SerializeField] private Arma arma;
    [SerializeField] private TipoMunicion tipoMunicion;
    [SerializeField] private int cantidad;
    [SerializeField] private bool reservado;

    public Arma Arma { get { return arma; } }
    public TipoMunicion TipoMunicion { get { return tipoMunicion; } }
    public int Cantidad { get { return cantidad; } }
    public bool Reservado { get { return reservado; } }

    // Sin arma, sin munición y sin reservar
    public bool EstaVacio { get { return arma == null && cantidad == 0 && !reservado; } }

    // Tiene munición guardada
    public bool TieneMunicion { get { return arma == null && cantidad > 0; } }

    // Cuántas unidades de un tipo de munición caben en un slot
    public static int Capacidad(TipoMunicion tipo)
    {
        if (tipo == TipoMunicion.Cargador) return CapacidadCargadores;
        return CapacidadCartuchos;
    }

    // Guarda munición en este slot. Devuelve la cantidad que no cupo.
    public int AgregarMunicion(TipoMunicion tipo, int cantidadNueva)
    {
        if (arma != null || reservado) return cantidadNueva;            // ocupado por un arma
        if (cantidad > 0 && tipoMunicion != tipo) return cantidadNueva; // tiene otro tipo de munición

        tipoMunicion = tipo;
        int espacioLibre = Capacidad(tipo) - cantidad;
        int guardada = Mathf.Min(espacioLibre, cantidadNueva);
        cantidad += guardada;
        return cantidadNueva - guardada;
    }

    // Saca munición de este slot. Devuelve cuánta se sacó.
    public int QuitarMunicion(int cantidadPedida)
    {
        int sacada = Mathf.Min(cantidad, cantidadPedida);
        cantidad -= sacada;
        return sacada;
    }

    // Guarda un arma en este slot (lo usa el inventario al recoger armas)
    public void GuardarArma(Arma armaNueva)
    {
        Vaciar();
        arma = armaNueva;
    }

    // Marca el slot como ocupado por un arma que ocupa varios espacios
    public void Reservar()
    {
        Vaciar();
        reservado = true;
    }

    public void Vaciar()
    {
        arma = null;
        cantidad = 0;
        reservado = false;
    }
}
