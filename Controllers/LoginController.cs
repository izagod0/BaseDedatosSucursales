using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;

namespace restauranteswebsbasededatos.Controllers
{
    public class LoginController : Controller
    {
        private string connectionString = "server=localhost;port=3305;user=root;password=12345;database=restaurante_mysql;";

        public IActionResult Index()
        {
            // Si ya hay sesión activa, redirigir según el rol
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol == "Gerente")
                return RedirectToAction("SeleccionarSucursal", "Gerente");
            if (rol == "Cajero")
                return RedirectToAction("Index", "PuntoVenta");
            
            return View();
        }

        [HttpPost]
        public IActionResult Index(string email, string password)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT u.id, u.nombre, u.email, u.password, r.nombre as rol, u.sucursal_id
                        FROM usuarios u
                        JOIN roles r ON u.rol_id = r.id
                        WHERE u.email = @email AND u.activo = 1";
                    
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@email", email);
                    
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string dbPassword = reader.GetString("password");
                            
                            if (dbPassword == password)
                            {
                                string rol = reader.GetString("rol");
                                int usuarioId = reader.GetInt32("id");
                                string nombre = reader.GetString("nombre");
                                int? sucursalId = reader.IsDBNull(reader.GetOrdinal("sucursal_id")) ? null : reader.GetInt32("sucursal_id");
                                
                                // Guardar sesión
                                HttpContext.Session.SetString("UsuarioNombre", nombre);
                                HttpContext.Session.SetString("UsuarioEmail", email);
                                HttpContext.Session.SetString("UsuarioRol", rol);
                                HttpContext.Session.SetInt32("UsuarioId", usuarioId);
                                
                                if (sucursalId.HasValue)
                                    HttpContext.Session.SetInt32("UsuarioSucursalId", sucursalId.Value);
                                
                                // Redirigir según rol
                                if (rol == "Gerente")
                                    return RedirectToAction("SeleccionarSucursal", "Gerente");
                                else
                                    return RedirectToAction("Index", "PuntoVenta");
                            }
                        }
                    }
                }
                
                ViewBag.Error = "Email o contraseña incorrectos";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error: " + ex.Message;
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}