using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ACMS_Web_Service.Data;
using ACMS_Web_Service.Models;

namespace ACMS_Web_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnitsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UnitsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<Unit>> Get() => await _context.Units.ToListAsync();

        [HttpPost]
        public async Task<ActionResult<Unit>> Post(Unit item)
        {
            _context.Units.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }
    }
}