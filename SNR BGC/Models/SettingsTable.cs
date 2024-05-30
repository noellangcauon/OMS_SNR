using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class SettingsTable
    {
        [Key]
        public int id { get; set; }
        public bool firstReprint { get; set; }
    }
   
}
