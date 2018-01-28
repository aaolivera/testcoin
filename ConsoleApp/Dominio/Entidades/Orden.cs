namespace Dominio.Entidades
{
    public class Orden
    {
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public override string ToString()
        {
            return "Orden: c:" + Cantidad + " p:" + PrecioUnitario;
        }
    }
}
