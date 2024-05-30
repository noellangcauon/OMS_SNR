using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class SupervisorTable
    {
        [Key]
        public int id { get; set; }
        public string supervisorId { get; set; }
        public string? supervisorName { get; set; }
        public string pin { get; set; }

    }
   
}
