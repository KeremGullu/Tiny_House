using Microsoft.AspNetCore.Mvc;
using Veritabanı_proje.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Veritabanı_proje.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || userRole != "0") // Admin rolü kontrolü
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            // İstatistikleri getir
            var totalHouses = await _context.TinyHouses.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalReservations = await _context.Reservations.CountAsync();

            ViewBag.TotalHouses = totalHouses;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalReservations = totalReservations;

            return View();
        }
    }
}