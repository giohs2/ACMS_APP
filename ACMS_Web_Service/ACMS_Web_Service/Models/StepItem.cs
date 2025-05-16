namespace ACMS_Web_Service.Models
{
    public class StepItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int DurationSeconds { get; set; }
        public int FinalTemperature { get; set; }
        public bool IsSteamerActive { get; set; }
        public bool TerminateIfTemperatureReached { get; set; }
        public bool TerminateIfTimeExpired { get; set; }
    }
}