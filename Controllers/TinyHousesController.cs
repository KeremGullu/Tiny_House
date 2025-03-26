using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Veritabanı_proje.Data;
using Veritabanı_proje.Models;
using System.Linq;

namespace Veritabanı_proje.Controllers
{
    public class TinyHousesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TinyHousesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var houses = await _context.TinyHouses
                .Include(h => h.Owner)
                .Where(h => h.IsActive)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();

            return View(houses);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var house = await _context.TinyHouses
                .Include(h => h.Owner)
                .Include(h => h.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (house == null)
            {
                return NotFound();
            }

            if (house.Reviews != null)
            {
                await _context.Entry(house)
                    .Collection(h => h.Reviews)
                    .Query()
                    .Include(r => r.User)
                    .LoadAsync();
            }

            return View(house);
        }

        // ... diğer action'lar ...
    }
}