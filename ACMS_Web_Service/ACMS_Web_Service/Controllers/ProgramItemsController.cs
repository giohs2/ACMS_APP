using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ACMS_Web_Service.Data;
using ACMS_Web_Service.Models;

namespace ACMS_Web_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProgramItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProgramItemsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<ProgramItem>> Get() => await _context.ProgramItems.Include(x => x.Steps).ToListAsync();

        [HttpPost]
        public async Task<ActionResult<ProgramItem>> Post(ProgramItem item)
        {
            _context.ProgramItems.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }
    }
}