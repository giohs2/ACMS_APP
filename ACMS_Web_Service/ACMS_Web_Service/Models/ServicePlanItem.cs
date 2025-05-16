using System.Collections.Generic;

namespace ACMS_Web_Service.Models
{
    public class ServicePlanItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? FlightId { get; set; }
        public Flight? Flight { get; set; }
        public ICollection<Cycle> Cycles { get; set; } = new List<Cycle>();
    }
}