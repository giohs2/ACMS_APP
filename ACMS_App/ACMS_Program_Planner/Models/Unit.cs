using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACMS_Program_Planner.Models
{
    public class Unit
    {
        public int UnitNumber { get; set; }

        public string? UnitName { get; set; }

        public byte DeviceClass { get; set; }

        public byte DeviceGroup { get; set; }

        public int DeviceId { get; set; }
    }
}
