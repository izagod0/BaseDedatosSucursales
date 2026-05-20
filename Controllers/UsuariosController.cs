using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using restauranteswebsbasededatos.Models;
using restauranteswebsbasededatos.Helpers;
using System.Collections.Generic;

namespace restauranteswebsbasededatos.Controllers
{
    [AuthorizeHelper("Gerente")]
    public class UsuariosController : Controller
    {
        private string connectionString = "server=localhost;port=3305;user=root;password=12345;database=restaurante_mysql;";

        public IActionResult Index()
        {
            List<Usuario> usuarios = new List<Usuario>();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT u.id, u.nombre, u.email, u.rol_id, r.nombre as rol_nombre, u.sucursal_id, s.nombre as sucursal_nombre, u.activo
                    FROM usuarios u
                    JOIN roles r ON u.rol_id = r.id
                    LEFT JOIN sucursales s ON u.sucursal_id = s.id";
                
                MySqlCommand cmd = new MySqlCommand(query, conn);
                
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        usuarios.Add(new Usuario
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre"),
                            Email = reader.GetString("email"),
                            RolId = reader.GetInt32("rol_id"),
                            RolNombre = reader.GetString("rol_nombre"),
                            SucursalId = reader.IsDBNull(reader.GetOrdinal("sucursal_id")) ? null : reader.GetInt32("sucursal_id"),
                            SucursalNombre = reader.IsDBNull(reader.GetOrdinal("sucursal_nombre")) ? "" : reader.GetString("sucursal_nombre"),
                            Activo = reader.GetBoolean("activo")
                        });
                    }
                }
            }

            return View(usuarios);
        }

        public IActionResult Create()
        {
            CargarRolesYSucursales();
            return View();
        }

        [HttpPost]
        public IActionResult Create(string nombre, string email, string password, int rolId, int? sucursalId)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    INSERT INTO usuarios (nombre, email, password, rol_id, sucursal_id, activo) 
                    VALUES (@nombre, @email, @password, @rol_id, @sucursal_id, 1)";
                
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@nombre", nombre);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.Parameters.AddWithValue("@rol_id", rolId);
                cmd.Parameters.AddWithValue("@sucursal_id", sucursalId ?? (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            Usuario usuario = null;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id, nombre, email, rol_id, sucursal_id FROM usuarios WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        usuario = new Usuario
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre"),
                            Email = reader.GetString("email"),
                            RolId = reader.GetInt32("rol_id"),
                            SucursalId = reader.IsDBNull(reader.GetOrdinal("sucursal_id")) ? null : reader.GetInt32("sucursal_id")
                        };
                    }
                }
            }

            CargarRolesYSucursales();
            return View(usuario);
        }

        [HttpPost]
        public IActionResult Edit(int id, string nombre, string email, int rolId, int? sucursalId)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    UPDATE usuarios SET nombre = @nombre, email = @email, rol_id = @rolId, sucursal_id = @sucursalId 
                    WHERE id = @id";
                
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@nombre", nombre);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@rolId", rolId);
                cmd.Parameters.AddWithValue("@sucursalId", sucursalId ?? (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM usuarios WHERE id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        private void CargarRolesYSucursales()
        {
            // Cargar roles
            List<Rol> roles = new List<Rol>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT id, nombre FROM roles", conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        roles.Add(new Rol { Id = reader.GetInt32("id"), Nombre = reader.GetString("nombre") });
                    }
                }
            }
            ViewBag.Roles = roles;

            // Cargar sucursales
            List<Sucursal> sucursales = new List<Sucursal>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT id, nombre FROM sucursales", conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sucursales.Add(new Sucursal { Id = reader.GetInt32("id"), Nombre = reader.GetString("nombre") });
                    }
                }
            }
            ViewBag.Sucursales = sucursales;
        }

        public IActionResult Prueba()
        {
            return Content("UsuariosController funciona", "text/plain");
        }
    }
}