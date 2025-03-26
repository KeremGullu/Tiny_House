using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Veritabanı_proje.Models;
using Microsoft.Data.SqlClient;

namespace Veritabanı_proje.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult TestConnection()
    {
        try
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                ViewBag.Message = "Veritabanı bağlantısı başarılı!";
            }
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Bağlantı hatası: {ex.Message}";
        }
        return View("Index");
    }
}
