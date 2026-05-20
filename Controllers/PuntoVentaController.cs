using Microsoft.AspNetCore.Mvc;
using restauranteswebsbasededatos.Helpers;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace restauranteswebsbasededatos.Controllers
{
    [AuthorizeHelper("Cajero", "Gerente")]
    public class PuntoVentaController : Controller
    {
        private string connectionString = "server=localhost;port=3305;user=root;password=12345;database=restaurante_mysql;";

        public IActionResult Index()
        {
            var usuarioNombre = HttpContext.Session.GetString("UsuarioNombre");
            var sucursalId = HttpContext.Session.GetInt32("UsuarioSucursalId") ?? 0;
            
            ViewBag.UsuarioNombre = usuarioNombre;
            ViewBag.SucursalId = sucursalId;
            return View();
        }

        // ← MÉTODO GET PARA OBTENER PRODUCTOS
        [HttpGet]
        public IActionResult GetProductos()
        {
            var sucursalId = HttpContext.Session.GetInt32("UsuarioSucursalId") ?? 0;
            var productos = new List<object>();
            
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT p.id, p.nombre, p.precio_venta, i.cantidad as stock
                    FROM inventario i
                    JOIN productos p ON i.producto_id = p.id
                    WHERE i.sucursal_id = @sucursalId AND i.cantidad > 0";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sucursalId", sucursalId);
                
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        productos.Add(new
                        {
                            id = reader.GetInt32("id"),
                            nombre = reader.GetString("nombre"),
                            precio = reader.GetDecimal("precio_venta"),
                            stock = reader.GetInt32("stock")
                        });
                    }
                }
            }
            return Json(productos);
        }

        // ← MÉTODO POST PARA REALIZAR VENTA
        [HttpPost]
        public IActionResult RealizarVenta([FromBody] VentaRequest request)
        {
            try
            {
                decimal total = 0;
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    foreach (var item in request.items)
                    {
                        decimal precio = item.precio;
                        decimal subtotal = precio * item.cantidad;
                        total += subtotal;
                        
                        string queryVenta = "INSERT INTO ventas (sucursal_id, producto_id, cantidad, total, fecha) VALUES (@sucursalId, @productoId, @cantidad, @subtotal, NOW())";
                        MySqlCommand cmdVenta = new MySqlCommand(queryVenta, conn);
                        cmdVenta.Parameters.AddWithValue("@sucursalId", request.sucursalId);
                        cmdVenta.Parameters.AddWithValue("@productoId", item.id);
                        cmdVenta.Parameters.AddWithValue("@cantidad", item.cantidad);
                        cmdVenta.Parameters.AddWithValue("@subtotal", subtotal);
                        cmdVenta.ExecuteNonQuery();
                        
                        string queryUpdate = "UPDATE inventario SET cantidad = cantidad - @cantidad WHERE sucursal_id = @sucursalId AND producto_id = @productoId";
                        MySqlCommand cmdUpdate = new MySqlCommand(queryUpdate, conn);
                        cmdUpdate.Parameters.AddWithValue("@cantidad", item.cantidad);
                        cmdUpdate.Parameters.AddWithValue("@sucursalId", request.sucursalId);
                        cmdUpdate.Parameters.AddWithValue("@productoId", item.id);
                        cmdUpdate.ExecuteNonQuery();
                    }
                }
                return Json(new { success = true, total = total });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    // ← CLASES FUERA DEL CONTROLADOR
    public class VentaRequest
    {
        public List<VentaItem> items { get; set; } = new List<VentaItem>();
        public int sucursalId { get; set; }
    }

    public class VentaItem
    {
        public int id { get; set; }
        public string nombre { get; set; } = "";
        public decimal precio { get; set; }
        public int cantidad { get; set; }
    }
}