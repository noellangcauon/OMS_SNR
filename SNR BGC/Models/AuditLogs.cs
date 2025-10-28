using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class AuditLogs
    {
        [Key]
        public int Id { get; set; }
        public int PerformedById { get; set; }
        public int UserId { get; set; }
        public string Module { get; set; }
        public string Action { get; set; }
        public string Field { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime DateCreated { get; set; }


    }
   
}
