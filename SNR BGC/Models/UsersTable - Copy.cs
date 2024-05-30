using System;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class IPaddressForAutoReload
    {
        [Key]
        public int id { get; set; }
        public string IPaddress { get; set; }
        public string? Name { get; set; }
    }
}
