using System;

namespace restauranteswebsbasededatos.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public int RolId { get; set; }
        public string RolNombre { get; set; } = "";
        public int? SucursalId { get; set; }
        public string SucursalNombre { get; set; } = "";
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
    }
}