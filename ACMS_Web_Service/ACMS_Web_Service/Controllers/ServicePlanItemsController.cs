using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ACMS_Web_Service.Data;
using ACMS_Web_Service.Models;

namespace ACMS_Web_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicePlanItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServicePlanItemsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<ServicePlanItem>> Get() => await _context.ServicePlanItems.Include(x => x.Cycles).ToListAsync();

        [HttpPost]
        public async Task<ActionResult<ServicePlanItem>> Post(ServicePlanItem item)
        {
            _context.ServicePlanItems.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }
    }
}