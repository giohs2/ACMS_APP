using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ACMS_Web_Service.Data;
using ACMS_Web_Service.Models;

namespace ACMS_Web_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StepItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StepItemsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<StepItem>> Get() => await _context.StepItems.ToListAsync();

        [HttpPost]
        public async Task<ActionResult<StepItem>> Post(StepItem item)
        {
            _context.StepItems.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }
    }
}