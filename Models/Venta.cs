using System;

namespace restauranteswebsbasededatos.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public int SucursalId { get; set; }
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
        public DateTime Fecha { get; set; }
    }
}