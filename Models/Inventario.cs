using System;

namespace restauranteswebsbasededatos.Models
{
    public class Inventario
    {
        public int Id { get; set; }
        public int SucursalId { get; set; }
        public string SucursalNombre { get; set; } = "";
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Subtotal => Cantidad * Precio;
        public DateTime FechaCaducidad { get; set; }
        public int DiasRestantes { get; set; }
        public string Alerta { get; set; } = "";
    }
}