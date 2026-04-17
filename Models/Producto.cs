using System;

namespace restauranteswebsbasededatos.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public decimal Precio { get; set; }
        public DateTime FechaCaducidad { get; set; }
        public int Stock { get; set; }
        
        public int DiasRestantes
        {
            get
            {
                return (FechaCaducidad.Date - DateTime.Today).Days;
            }
        }
        
        public string ColorAlerta
        {
            get
            {
                if (DiasRestantes <= 0) return "#ffcccc";
                if (DiasRestantes <= 7) return "#ffe6cc";
                return "#ffffff";
            }
        }
    }
}