using System;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class UsersTable
    {
        [Key]
        public int userId { get; set; }
        [Required]
        public string username { get; set; }
        public string? password { get; set; }
        public string? userFullname { get; set; }
        public string? userRole { get; set; }
        public string? accessType { get; set; }

        public bool? withOmsAccess { get; set; }
        public bool? withRunnerAccess { get; set; }
        public bool? withPickerAccess { get; set; }
        public string? OmsSubModule { get; set; }
        public string? OmsPrivilege { get; set; }
        public string? userStatus { get; set; }
        public DateTime? lastEditDate { get; set; }
        public string? lastEditUser { get; set; }
        public string? lastEditColumn { get; set; }
        public string? lastEditValue { get; set; }
        public bool? withBoxerAccess { get; set; }
        public bool? newUser { get; set; }
        public string? employeeId { get; set; }
        public int? failedAttempts { get; set; }
        public DateTime? passwordExpiration { get; set; }
        public string email { get; set; }
    }
}
