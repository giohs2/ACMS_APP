using System.Collections.Generic;

namespace ACMS_Web_Service.Models
{
    public class Flight
    {
        public int Id { get; set; }
        public string? FlightNumber { get; set; }
        public string? FlightDate { get; set; }
        public string? FlightTime { get; set; }
        public ICollection<Unit> Units { get; set; } = new List<Unit>();
    }
}