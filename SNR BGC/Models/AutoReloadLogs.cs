using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SNR_BGC.Models
{
    public class AutoReloadLogs
    {
        [Key]
        public int id { get; set; }
        public string? logs { get; set; }
        public string platform { get; set; }
        public DateTime? dateProcess { get; set; }
        public DateTime? completion { get; set; }
        public int totalOrder { get; set; }
        public string status { get; set; }
        public string? coverage { get; set; }
        public string agent { get; set; }
        public string? GetOrdersDone { get; set; }
        public string? GetDispatchDone { get; set; }
        public DateTime? coverageFrom { get; set; }
        public DateTime? coverageTo { get; set; }
        public bool? fromFailed { get; set; }
    }
}
