using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ACMS_Web_Service.Data;
using ACMS_Web_Service.Models;

namespace ACMS_Web_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CyclesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CyclesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<Cycle>> Get() => await _context.Cycles.Include(x => x.UnitPrograms).ToListAsync();

        [HttpPost]
        public async Task<ActionResult<Cycle>> Post(Cycle item)
        {
            _context.Cycles.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }
    }
}