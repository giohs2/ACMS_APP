namespace ACMS_Web_Service.Models
{
    public class UnitProgram
    {
        public int Id { get; set; }
        public Unit? Unit { get; set; }
        public string? Name { get; set; }
        public ProgramItem? Program { get; set; }
        public Cycle? Cycle { get; set; }
    }
}