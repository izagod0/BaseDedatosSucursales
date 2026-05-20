using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using restauranteswebsbasededatos.Models;
using restauranteswebsbasededatos.Helpers;
using System.Collections.Generic;

namespace restauranteswebsbasededatos.Controllers
{
    [AuthorizeHelper("Gerente")]
    public class InventarioController : Controller
    {
        private string connectionString = "server=localhost;port=3305;user=root;password=12345;database=restaurante_mysql;";

        // Mostrar formulario para seleccionar sucursal
        public IActionResult Index()
        {
            List<Sucursal> sucursales = new List<Sucursal>();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id, nombre FROM sucursales";
                MySqlCommand cmd = new MySqlCommand(query, conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sucursales.Add(new Sucursal
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre")
                        });
                    }
                }
            }

            return View(sucursales);
        }

        // Ver inventario de una sucursal específica
        public IActionResult VerInventario(int id)
        {
            List<Inventario> inventario = new List<Inventario>();
            string nombreSucursal = "";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                
                // Obtener nombre de la sucursal
                string querySucursal = "SELECT nombre FROM sucursales WHERE id = @id";
                MySqlCommand cmdSucursal = new MySqlCommand(querySucursal, conn);
                cmdSucursal.Parameters.AddWithValue("@id", id);
                nombreSucursal = cmdSucursal.ExecuteScalar()?.ToString() ?? "";

                // Obtener inventario
                string query = @"
                    SELECT i.id, i.sucursal_id, i.producto_id, i.cantidad, 
                           p.nombre as producto_nombre, p.precio_venta
                    FROM inventario i
                    JOIN productos p ON i.producto_id = p.id
                    WHERE i.sucursal_id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        inventario.Add(new Inventario
                        {
                            Id = reader.GetInt32("id"),
                            SucursalId = id,
                            ProductoId = reader.GetInt32("producto_id"),
                            ProductoNombre = reader.GetString("producto_nombre"),
                            Cantidad = reader.GetInt32("cantidad"),
                            Precio = reader.GetDecimal("precio_venta")
                        });
                    }
                }
            }

            ViewBag.SucursalNombre = nombreSucursal;
            ViewBag.SucursalId = id;
            return View(inventario);
        }
    }
}