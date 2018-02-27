
using Dominio.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio.Entidades
{
    public class Relacion
    {
        [Key]
        public string Nombre { get; set; }
        public Moneda Principal { get; set; }
        public Moneda Secundaria { get; set; }
        public DateTime FechaDeActualizacion { get; set; }
        
        public decimal MayorPrecioDeVentaAjecutada { get; set; }
        public decimal Volumen { get; set; }
        public decimal Compra { get; set; }
        public decimal Venta { get; set; }
        public decimal DeltaEjecutado { get; set; }
        public decimal DeltaActual { get; set; }

        public void CalcularDeltas()
        {
            DeltaEjecutado = Math.Round((MayorPrecioDeVentaAjecutada - Compra) * 100 / (MayorPrecioDeVentaAjecutada == 0 ? 1 : MayorPrecioDeVentaAjecutada), 0);
            DeltaActual = Math.Round((Venta - Compra) * 100 / (Venta == 0 ? 1 : Venta), 0);
        }

        [NotMapped]
        public ICollection<Orden> Compras { get; }
        [NotMapped]
        public ICollection<Orden> Ventas { get; }

        public void AgregarOrden(Orden orden)
        {
            if (orden.EsDeVenta)
            {
                Ventas.Add(orden);
            }
            else
            {
                Compras.Add(orden);
            }
        }
    }
}
