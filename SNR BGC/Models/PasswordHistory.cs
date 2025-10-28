using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SNR_BGC.Models
{
    public class PasswordHistory
    {
        [Key]
        public int Id { get; set; }
        public int userId { get; set; }
        public string Password { get; set; }
        public DateTime DateCreated { get; set; }


    }
   
}
