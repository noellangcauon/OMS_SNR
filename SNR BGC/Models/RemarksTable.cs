using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SNR_BGC.Models
{
    public class RemarksTable
    {
        [Key]
        public int code { get; set; }
        public string description { get; set; }
    }
}
