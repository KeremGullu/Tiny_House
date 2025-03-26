using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veritabanı_proje.Models;
using Veritabanı_proje.Data;
using Veritabanı_proje.Attributes;
using Veritabanı_proje.Models.ViewModels;

namespace Veritabanı_proje.Controllers
{
    [AuthorizeRoles(Role.EvSahibi)]
    public class EvSahibiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EvSahibiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EvSahibi/Index
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users
                .Include(u => u.OwnedHouses)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new EvSahibiDashboardViewModel
            {
                User = user,
                OwnedHouses = user.OwnedHouses?.ToList() ?? new List<TinyHouse>()
            };

            return View(viewModel);
        }

        // GET: EvSahibi/TinyHouses
        public async Task<IActionResult> TinyHouses()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                Console.WriteLine("User ID is null in TinyHouses action");
                return RedirectToAction("Login", "Account");
            }

            // Kullanıcının evlerini getir
            var houses = await _context.TinyHouses
                .Where(t => t.OwnerId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            Console.WriteLine($"Found {houses.Count} houses for user ID: {userId}");
            foreach (var house in houses)
            {
                Console.WriteLine($"House ID: {house.Id}, Name: {house.Name}, Owner ID: {house.OwnerId}");
            }

            return View(houses);
        }

        // GET: EvSahibi/CreateTinyHouse
        public IActionResult CreateTinyHouse()
        {
            return View();
        }

        // POST: EvSahibi/CreateTinyHouse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTinyHouse(TinyHouse house, List<IFormFile> photos)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                house.OwnerId = userId.Value;
                house.CreatedAt = DateTime.Now;
                house.IsActive = true;

                // Fotoğrafları kaydet
                if (photos != null && photos.Any())
                {
                    var photoList = new List<string>();
                    foreach (var photo in photos)
                    {
                        if (photo.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                            var filePath = Path.Combine("wwwroot/uploads", fileName);
                            
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await photo.CopyToAsync(stream);
                            }
                            
                            photoList.Add("/uploads/" + fileName);
                        }
                    }
                    house.Photos = System.Text.Json.JsonSerializer.Serialize(photoList);
                }

                // Özellikleri JSON olarak kaydet
                if (Request.Form.TryGetValue("amenities", out var amenities))
                {
                    house.Amenities = System.Text.Json.JsonSerializer.Serialize(
                        amenities.ToString().Split(',').Select(a => a.Trim()).ToList()
                    );
                }

                _context.Add(house);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(house);
        }

        // GET: EvSahibi/Reservations
        public async Task<IActionResult> Reservations()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var reservations = await _context.Reservations
                .Include(r => r.TinyHouse)
                .Include(r => r.User)
                .Where(r => r.TinyHouse.OwnerId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reservations);
        }

        // POST: EvSahibi/UpdateReservationStatus
        [HttpPost]
        public async Task<IActionResult> UpdateReservationStatus(int reservationId, string status)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation != null)
            {
                reservation.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Reservations));
        }

        // GET: EvSahibi/Reviews
        public async Task<IActionResult> Reviews()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var reviews = await _context.Reviews
                .Include(r => r.TinyHouse)
                .Include(r => r.User)
                .Where(r => r.TinyHouse.OwnerId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var house = await _context.TinyHouses
                .FirstOrDefaultAsync(m => m.Id == id && m.OwnerId == userId);

            if (house == null)
            {
                return NotFound();
            }

            return View(house);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TinyHouse house, List<IFormFile> photos)
        {
            if (id != house.Id)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var existingHouse = await _context.TinyHouses
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == id);

            if (existingHouse == null || existingHouse.OwnerId != userId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    house.OwnerId = userId.Value;
                    house.UpdatedAt = DateTime.Now;

                    // Mevcut fotoğrafları koru
                    var existingPhotos = existingHouse.Photos;

                    // Yeni fotoğrafları kaydet
                    if (photos != null && photos.Any())
                    {
                        var photoList = existingPhotos != null 
                            ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(existingPhotos) 
                            : new List<string>();

                        foreach (var photo in photos)
                        {
                            if (photo.Length > 0)
                            {
                                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                                var filePath = Path.Combine("wwwroot/uploads", fileName);
                                
                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await photo.CopyToAsync(stream);
                                }
                                
                                photoList.Add("/uploads/" + fileName);
                            }
                        }
                        house.Photos = System.Text.Json.JsonSerializer.Serialize(photoList);
                    }
                    else
                    {
                        house.Photos = existingPhotos;
                    }

                    // Özellikleri JSON olarak kaydet
                    if (Request.Form.TryGetValue("amenities", out var amenities))
                    {
                        house.Amenities = System.Text.Json.JsonSerializer.Serialize(
                            amenities.ToString().Split(',').Select(a => a.Trim()).ToList()
                        );
                    }

                    _context.Update(house);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TinyHouseExists(house.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(house);
        }

        private bool TinyHouseExists(int id)
        {
            return _context.TinyHouses.Any(e => e.Id == id);
        }
    }
} 