using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyHotel.Data;
using MyHotel.Models;

namespace MyHotel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _db;



        public BookingController(ApplicationDbContext db)

        {

            _db = db;

        }

        [HttpGet]

        public async Task<ActionResult<IEnumerable<Room>>> GetHotels()

        {

            return await _db.Rooms.ToListAsync();

        }

    }
}
