using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SNR_BGC.Models
{
    public class AuthClass
    {
        [Key]
        public int ids { get; set; }
        public string authCode { get; set; }
        public DateTime? dateEntry { get; set; }
        public string module { get; set; }
    }
}
