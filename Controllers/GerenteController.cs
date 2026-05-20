using Microsoft.AspNetCore.Mvc;
using restauranteswebsbasededatos.Helpers;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using restauranteswebsbasededatos.Models;

namespace restauranteswebsbasededatos.Controllers
{
    [AuthorizeHelper("Gerente")]
    public class GerenteController : Controller
    {
        private string connectionString = "server=localhost;port=3305;user=root;password=12345;database=restaurante_mysql;";

        public IActionResult SeleccionarSucursal()
        {
            List<Sucursal> sucursales = new List<Sucursal>();
            
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id, nombre, direccion, telefono FROM sucursales";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sucursales.Add(new Sucursal
                        {
                            Id = reader.GetInt32("id"),
                            Nombre = reader.GetString("nombre"),
                            Direccion = reader.GetString("direccion"),
                            Telefono = reader.GetString("telefono")
                        });
                    }
                }
            }
            
            ViewBag.UsuarioNombre = HttpContext.Session.GetString("UsuarioNombre");
            return View(sucursales);
        }
        
        public IActionResult Dashboard(int id)
        {
            HttpContext.Session.SetInt32("UsuarioSucursalId", id);
            ViewBag.SucursalId = id;
            ViewBag.UsuarioNombre = HttpContext.Session.GetString("UsuarioNombre");
            return View();
        }

        public IActionResult PuntoVenta()
        {
            var sucursalId = HttpContext.Session.GetInt32("UsuarioSucursalId") ?? 0;
            ViewBag.SucursalId = sucursalId;
            ViewBag.UsuarioNombre = HttpContext.Session.GetString("UsuarioNombre");
            return View();
        }
    }
}