using System.ComponentModel.DataAnnotations;

namespace Dominio.Entidades
{
    public class Moneda
    {
        [Key]
        public string Nombre { get; set; }        
    }
}
