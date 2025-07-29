using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACMS_Program_Planner.Models
{
    public class Flight
    {
        public int FlightId { get; set; }
        public string? FlightNumber { get; set; }
        public string? FlightDate { get; set; }
        public string? FlightTime { get; set; }
        public string? CombinedFlightName => $"{FlightNumber} {FlightDate} {FlightTime}";

        public ObservableCollection<Unit> Units { get; set; } = new ObservableCollection<Unit>();

    }
}
