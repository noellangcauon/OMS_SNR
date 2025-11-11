using System;

namespace SNR_BGC.Models.UserAccount_AuditingTool
{
    public class PPH_Employee_Details
    {
        public string EmployeeCode { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Location { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string Reason { get; set; }
        public bool? EmploymentStatus { get; set; }
        public DateTime? DateSeparated { get; set; }
        public DateTime? DateDeactivated { get; set; }
        public string PositionLevel { get; set; }
        public string Company { get; set; }
    }
}
