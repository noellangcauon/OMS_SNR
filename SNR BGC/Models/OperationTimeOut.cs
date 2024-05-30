using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SNR_BGC.Models
{
    public class OperationTimeOut
    {
        [Key]
        public int Id { get; set; }
        public string description { get; set; }
        public TimeSpan timeOutFrom { get; set; }
        public TimeSpan timeOutTo { get; set; }
    }
}
