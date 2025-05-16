namespace ACMS_Web_Service.Models
{
    public class Unit
    {
        public int Id { get; set; }
        public int UnitNumber { get; set; }
        public string? UnitName { get; set; }
        public int DeviceClass { get; set; }
        public int DeviceGroup { get; set; }
        public int DeviceId { get; set; }
    }
}