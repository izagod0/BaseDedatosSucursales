using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using restauranteswebsbasededatos.Models;
using System;
using System.Collections.Generic;

namespace restauranteswebsbasededatos.Controllers
{
    public class ProductosController : Controller
    {
        private string connectionString = "server=localhost;port=3305;user=root;password=12345;database=restaurante_mysql;";

        // LISTAR productos
        public IActionResult Index()
        {
            List<Producto> productos = new List<Producto>();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id, nombre, precio_venta, fecha_caducidad, stock FROM productos";
                MySqlCommand cmd = new MySqlCommand(query, conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        productos.Add(new Producto
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre"),
                            Precio = reader.GetDecimal("precio_venta"),
                            FechaCaducidad = reader.GetDateTime("fecha_caducidad"),
                            Stock = reader.GetInt32("stock")
                        });
                    }
                }
            }

            return View(productos);
        }

        // FORMULARIO AGREGAR
        public IActionResult Create()
        {
            return View();
        }

        // AGREGAR producto
        [HttpPost]
        public IActionResult Create(Producto producto)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = @"INSERT INTO productos (nombre, precio_venta, fecha_caducidad, stock) 
                                 VALUES (@nombre, @precio, @fecha_caducidad, @stock)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@nombre", producto.Nombre);
                cmd.Parameters.AddWithValue("@precio", producto.Precio);
                cmd.Parameters.AddWithValue("@fecha_caducidad", producto.FechaCaducidad);
                cmd.Parameters.AddWithValue("@stock", producto.Stock);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        // FORMULARIO EDITAR
        public IActionResult Edit(int id)
        {
            Producto producto = null;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id, nombre, precio_venta, fecha_caducidad, stock FROM productos WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        producto = new Producto
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre"),
                            Precio = reader.GetDecimal("precio_venta"),
                            FechaCaducidad = reader.GetDateTime("fecha_caducidad"),
                            Stock = reader.GetInt32("stock")
                        };
                    }
                }
            }

            if (producto == null) return NotFound();
            return View(producto);
        }

        // EDITAR producto
        [HttpPost]
        public IActionResult Edit(Producto producto)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = @"UPDATE productos SET nombre = @nombre, precio_venta = @precio, 
                                 fecha_caducidad = @fecha_caducidad, stock = @stock WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", producto.Id);
                cmd.Parameters.AddWithValue("@nombre", producto.Nombre);
                cmd.Parameters.AddWithValue("@precio", producto.Precio);
                cmd.Parameters.AddWithValue("@fecha_caducidad", producto.FechaCaducidad);
                cmd.Parameters.AddWithValue("@stock", producto.Stock);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        // FORMULARIO ELIMINAR
        public IActionResult Delete(int id)
        {
            Producto producto = null;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id, nombre FROM productos WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        producto = new Producto
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre")
                        };
                    }
                }
            }

            if (producto == null) return NotFound();
            return View(producto);
        }

        // ELIMINAR producto
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM productos WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }
    }
}