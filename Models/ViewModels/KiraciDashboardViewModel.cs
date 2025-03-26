using Veritabanı_proje.Models;
using System.Collections.Generic;

namespace Veritabanı_proje.Models.ViewModels
{
    public class KiraciDashboardViewModel
    {
        public User? User { get; set; }
        public List<TinyHouse> AvailableHouses { get; set; } = new();
        public List<Reservation> UserReservations { get; set; } = new();
        public List<Review> UserReviews { get; set; } = new();
    }
} 