using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veritabanı_proje.Models;
using Veritabanı_proje.Models.ViewModels;
using Veritabanı_proje.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Veritabanı_proje.Attributes;
using System.Collections.Generic;

namespace Veritabanı_proje.Controllers
{
    [AuthorizeRoles(Role.Kiraci)]
    public class KiraciController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KiraciController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users
                .Include(u => u.Reservations)
                .ThenInclude(r => r.TinyHouse)
                .Include(u => u.Reviews)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var availableHouses = await _context.TinyHouses
                .Include(h => h.Owner)
                .Include(h => h.Reviews)
                .ThenInclude(r => r.User)
                .Where(h => h.IsActive)
                .ToListAsync();

            var viewModel = new KiraciDashboardViewModel
            {
                User = user,
                AvailableHouses = availableHouses,
                UserReservations = user.Reservations?.ToList() ?? new List<Reservation>(),
                UserReviews = user.Reviews?.ToList() ?? new List<Review>()
            };

            return View(viewModel);
        }

        // Ev detaylarını görüntüleme
        public async Task<IActionResult> HouseDetails(int id)
        {
            var house = await _context.TinyHouses
                .Include(h => h.Owner)
                .Include(h => h.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (house == null)
            {
                return NotFound();
            }

            return View(house);
        }

        // Rezervasyon yapma
        [HttpPost]
        public async Task<IActionResult> MakeReservation(int houseId, DateTime startDate, DateTime endDate)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var house = await _context.TinyHouses.FindAsync(houseId);
            if (house == null)
            {
                return NotFound();
            }

            // Tarih çakışması kontrolü
            var hasConflict = await _context.Reservations
                .AnyAsync(r => r.TinyHouseId == houseId &&
                              r.Status != "İptalEdildi" &&
                              ((startDate >= r.StartDate && startDate <= r.EndDate) ||
                               (endDate >= r.StartDate && endDate <= r.EndDate)));

            if (hasConflict)
            {
                TempData["Error"] = "Seçilen tarihler için rezervasyon yapılamaz.";
                return RedirectToAction("HouseDetails", new { id = houseId });
            }

            var totalDays = (endDate - startDate).Days;
            var totalPrice = house.Price * totalDays;

            var reservation = new Reservation
            {
                TinyHouseId = houseId,
                UserId = userId.Value,
                StartDate = startDate,
                EndDate = endDate,
                TotalPrice = totalPrice,
                Status = "Beklemede",
                CreatedAt = DateTime.Now
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Rezervasyon talebiniz alınmıştır.";
            return RedirectToAction("Index");
        }
    }
} 