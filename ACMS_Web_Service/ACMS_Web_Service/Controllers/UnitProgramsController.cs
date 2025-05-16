using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ACMS_Web_Service.Data;
using ACMS_Web_Service.Models;

namespace ACMS_Web_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnitProgramsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UnitProgramsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<UnitProgram>> Get() => await _context.UnitPrograms.Include(x => x.Unit).ToListAsync();

        [HttpPost]
        public async Task<ActionResult<UnitProgram>> Post(UnitProgram item)
        {
            _context.UnitPrograms.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }
    }
}