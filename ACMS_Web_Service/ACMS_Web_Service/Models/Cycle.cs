using System.Collections.Generic;

namespace ACMS_Web_Service.Models
{
    public class Cycle
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int ServicePlanItemId { get; set; }
        public ServicePlanItem? ServicePlanItem { get; set; }
        public ICollection<UnitProgram> UnitPrograms { get; set; } = new List<UnitProgram>();
        public double? DelaySeconds { get; set; }
    }
}