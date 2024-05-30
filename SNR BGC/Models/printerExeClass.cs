using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SNR_BGC.Models
{
    public class printerExeClass
    {
        [Key]
        public int ids { get; set; }
        public string filepath { get; set; }
        public string printer { get; set; }
        public string platform { get; set; }
    }
}
