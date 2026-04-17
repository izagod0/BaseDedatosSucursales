using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using restauranteswebsbasededatos.Models;
using System.Collections.Generic;

namespace restauranteswebsbasededatos.Controllers
{
    public class HomeController : Controller
    {
        private string connectionString = "server=localhost;port=3305;user=root;password=12345;database=restaurante_mysql;";

        public IActionResult Index()
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

            return View(sucursales);
        }
    }
}