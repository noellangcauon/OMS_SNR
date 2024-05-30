using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SNR_BGC.Models
{
    public class GridOrderHeaderClass
    {
        [Key]

        public int entryNum { get; set; }
        public string orderId { get; set; }
        public DateTime ? dateFetch { get; set; }
        public DateTime ? dateProcess { get; set; }
        public DateTime ? dateCreatedAt { get; set; }
        public string module { get; set; }
        public string status { get; set; }
        public int exception { get; set; }
        public string customerID { get; set; }
        public string employee { get; set; }
        public decimal item_count { get; set; }
        public decimal total_amount { get; set; }
        public string userClear { get; set; }
        public int exceptions_count { get; set; }
        public decimal exception_count { get; set; }
        public string tub_no { get; set; }
        public string typeOfException { get; set; }
        public string? username { get; set; }
        public string? pickerUser { get; set; }

        public int row_num { get; set; }



    }
}
