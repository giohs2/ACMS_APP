using System.Collections.Generic;

namespace ACMS_Web_Service.Models
{
    public class ProgramItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public ICollection<StepItem> Steps { get; set; } = new List<StepItem>();
    }
}