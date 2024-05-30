using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SNR_BGC.Models
{
    public class TokenClass
    {
        [Key]
        public int ids { get; set; }
        public string codeToGetToken { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string country_user_id { get; set; }
        public string country_seller_id { get; set; }
        public string country_short_code { get; set; }
        public string refresh_expires_in { get; set; }
        public string access_expires_in { get; set; }
        public DateTime? dateEntry { get; set; }
        public string module { get; set; }
    }
}
