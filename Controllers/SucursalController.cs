using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using restauranteswebsbasededatos.Models;
using System;
using System.Collections.Generic;

namespace restauranteswebsbasededatos.Controllers
{
    public class SucursalController : Controller
    {
        private string connectionString = "server=localhost;port=3305;user=root;password=12345;database=restaurante_mysql;";

        public IActionResult Dashboard(int id)
        {
            ViewBag.SucursalId = id;
            string nombreSucursal = "";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT nombre FROM sucursales WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                nombreSucursal = cmd.ExecuteScalar()?.ToString() ?? "";
            }

            ViewBag.SucursalNombre = nombreSucursal;
            return View();
        }

        public IActionResult Inventario(int id)
        {
            List<Inventario> inventario = new List<Inventario>();
            string nombreSucursal = "";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string querySucursal = "SELECT nombre FROM sucursales WHERE id = @id";
                MySqlCommand cmdSucursal = new MySqlCommand(querySucursal, conn);
                cmdSucursal.Parameters.AddWithValue("@id", id);
                nombreSucursal = cmdSucursal.ExecuteScalar()?.ToString() ?? "";

                string query = @"
                    SELECT p.id, p.nombre, p.precio_venta, p.fecha_caducidad, i.cantidad,
                           DATEDIFF(p.fecha_caducidad, CURDATE()) as dias_restantes
                    FROM inventario i
                    JOIN productos p ON i.producto_id = p.id
                    WHERE i.sucursal_id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int diasRestantes = reader.GetInt32("dias_restantes");
                        string alerta = "";
                        if (diasRestantes <= 0) alerta = "🔴 CADUCADO";
                        else if (diasRestantes <= 7) alerta = "🟠 POR CADUCAR (" + diasRestantes + " días)";
                        else alerta = "🟢 VIGENTE";

                        inventario.Add(new Inventario
                        {
                            ProductoId = reader.GetInt32("id"),
                            ProductoNombre = reader.GetString("nombre"),
                            Precio = reader.GetDecimal("precio_venta"),
                            Cantidad = reader.GetInt32("cantidad"),
                            FechaCaducidad = reader.GetDateTime("fecha_caducidad"),
                            DiasRestantes = diasRestantes,
                            Alerta = alerta
                        });
                    }
                }
            }

            ViewBag.SucursalNombre = nombreSucursal;
            ViewBag.SucursalId = id;
            return View(inventario);
        }

        public IActionResult AgregarProducto(int id)
        {
            ViewBag.SucursalId = id;
            List<Producto> productos = new List<Producto>();
            
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT id, nombre, precio_venta FROM productos 
                    WHERE id NOT IN (SELECT producto_id FROM inventario WHERE sucursal_id = @id)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        productos.Add(new Producto
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre"),
                            Precio = reader.GetDecimal("precio_venta")
                        });
                    }
                }
            }
            
            return View(productos);
        }

        [HttpPost]
        public IActionResult AgregarProducto(int sucursalId, int productoId, int cantidad)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO inventario (sucursal_id, producto_id, cantidad) VALUES (@sucursalId, @productoId, @cantidad)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sucursalId", sucursalId);
                cmd.Parameters.AddWithValue("@productoId", productoId);
                cmd.Parameters.AddWithValue("@cantidad", cantidad);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Inventario", new { id = sucursalId });
        }

        public IActionResult Vender(int id)
        {
            ViewBag.SucursalId = id;
            List<Inventario> inventario = new List<Inventario>();
            
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT p.id, p.nombre, p.precio_venta, i.cantidad
                    FROM inventario i
                    JOIN productos p ON i.producto_id = p.id
                    WHERE i.sucursal_id = @id AND i.cantidad > 0";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        inventario.Add(new Inventario
                        {
                            ProductoId = reader.GetInt32("id"),
                            ProductoNombre = reader.GetString("nombre"),
                            Precio = reader.GetDecimal("precio_venta"),
                            Cantidad = reader.GetInt32("cantidad")
                        });
                    }
                }
            }
            
            return View(inventario);
        }

        [HttpPost]
        public IActionResult Vender(int sucursalId, int productoId, int cantidad)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string queryPrecio = "SELECT precio_venta FROM productos WHERE id = @id";
                MySqlCommand cmdPrecio = new MySqlCommand(queryPrecio, conn);
                cmdPrecio.Parameters.AddWithValue("@id", productoId);
                decimal precio = Convert.ToDecimal(cmdPrecio.ExecuteScalar());
                decimal total = precio * cantidad;

                string queryUpdate = "UPDATE inventario SET cantidad = cantidad - @cantidad WHERE sucursal_id = @sucursalId AND producto_id = @productoId";
                MySqlCommand cmdUpdate = new MySqlCommand(queryUpdate, conn);
                cmdUpdate.Parameters.AddWithValue("@cantidad", cantidad);
                cmdUpdate.Parameters.AddWithValue("@sucursalId", sucursalId);
                cmdUpdate.Parameters.AddWithValue("@productoId", productoId);
                cmdUpdate.ExecuteNonQuery();

                string queryVenta = "INSERT INTO ventas (sucursal_id, producto_id, cantidad, total, fecha) VALUES (@sucursalId, @productoId, @cantidad, @total, NOW())";
                MySqlCommand cmdVenta = new MySqlCommand(queryVenta, conn);
                cmdVenta.Parameters.AddWithValue("@sucursalId", sucursalId);
                cmdVenta.Parameters.AddWithValue("@productoId", productoId);
                cmdVenta.Parameters.AddWithValue("@cantidad", cantidad);
                cmdVenta.Parameters.AddWithValue("@total", total);
                cmdVenta.ExecuteNonQuery();
            }
            return RedirectToAction("Inventario", new { id = sucursalId });
        }

        public IActionResult Reporte(int id)
        {
            ViewBag.SucursalId = id;
            decimal totalVentas = 0;
            decimal valorInventario = 0;
            List<Venta> ventas = new List<Venta>();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string queryVentas = "SELECT IFNULL(SUM(total), 0) as total FROM ventas WHERE sucursal_id = @id";
                MySqlCommand cmdVentas = new MySqlCommand(queryVentas, conn);
                cmdVentas.Parameters.AddWithValue("@id", id);
                totalVentas = Convert.ToDecimal(cmdVentas.ExecuteScalar());

                string queryInventario = @"
                    SELECT IFNULL(SUM(i.cantidad * p.precio_venta), 0) as total 
                    FROM inventario i 
                    JOIN productos p ON i.producto_id = p.id 
                    WHERE i.sucursal_id = @id";
                MySqlCommand cmdInventario = new MySqlCommand(queryInventario, conn);
                cmdInventario.Parameters.AddWithValue("@id", id);
                valorInventario = Convert.ToDecimal(cmdInventario.ExecuteScalar());

                string queryUltimasVentas = @"
                    SELECT v.id, p.nombre, v.cantidad, v.total, v.fecha
                    FROM ventas v
                    JOIN productos p ON v.producto_id = p.id
                    WHERE v.sucursal_id = @id
                    ORDER BY v.fecha DESC
                    LIMIT 10";
                MySqlCommand cmdUltimas = new MySqlCommand(queryUltimasVentas, conn);
                cmdUltimas.Parameters.AddWithValue("@id", id);
                
                using (MySqlDataReader reader = cmdUltimas.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ventas.Add(new Venta
                        {
                            Id = reader.GetInt32("id"),
                            ProductoNombre = reader.GetString("nombre"),
                            Cantidad = reader.GetInt32("cantidad"),
                            Total = reader.GetDecimal("total"),
                            Fecha = reader.GetDateTime("fecha")
                        });
                    }
                }
            }

            ViewBag.TotalVentas = totalVentas;
            ViewBag.ValorInventario = valorInventario;
            ViewBag.SucursalNombre = ObtenerNombreSucursal(id);
            return View(ventas);
        }

        private string ObtenerNombreSucursal(int id)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT nombre FROM sucursales WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                return cmd.ExecuteScalar()?.ToString() ?? "";
            }
        }
    }
}