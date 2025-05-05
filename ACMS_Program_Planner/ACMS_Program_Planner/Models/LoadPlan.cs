using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACMS_Program_Planner.Models
{
    public class LoadPlan
    {
        public Flight? Flight { get; set; }
        public int GalleyBlockId { get; set; }
        public string? FlightName => Flight?.CombinedFlightName;
    }
}
